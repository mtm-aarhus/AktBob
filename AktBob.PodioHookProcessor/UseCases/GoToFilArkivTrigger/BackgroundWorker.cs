using AAK.Podio;
using AktBob.DatabaseAPI.Contracts.Queries;
using AktBob.Deskpro.Contracts;
using AktBob.Podio.Contracts;
using AktBob.Queue.Contracts;
using AktBob.UiPath.Contracts;
using Ardalis.GuardClauses;
using MassTransit;
using MassTransit.Mediator;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AktBob.PodioHookProcessor.UseCases.GoToFilArkivTrigger;
internal class BackgroundWorker : BackgroundService
{
    private ILogger<BackgroundWorker> _logger;
    private IConfiguration _configuration;
    private readonly IPodio _podio;

    public IServiceProvider ServiceProvider { get; }

    public BackgroundWorker(ILogger<BackgroundWorker> logger, IConfiguration configuration, IServiceProvider serviceProvider, IPodio podio)
    {
        _logger = logger;
        _configuration = configuration;
        ServiceProvider = serviceProvider;
        _podio = podio;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var tenancyName = Guard.Against.NullOrEmpty(_configuration.GetValue<string>("UiPath:TenancyName"));
        var azureQueueName = Guard.Against.NullOrEmpty(_configuration.GetValue<string>($"GoToFilArkivTrigger:AzureQueueName"));
        var uiPathQueueName = Guard.Against.NullOrEmpty(_configuration.GetValue<string>($"GoToFilArkivTrigger:UiPathQueueName:{tenancyName}"));
        var delay = _configuration.GetValue<int?>("GoToFilArkivTrigger:WorkerIntervalSeconds") ?? 10;
        var podioAppId = Guard.Against.Null(_configuration.GetValue<int?>("Podio:AppId"));
        var podioFields = Guard.Against.Null(Guard.Against.NullOrEmpty(_configuration.GetSection("Podio:Fields").GetChildren().ToDictionary(x => long.Parse(x.Key), x => x.Get<PodioField>())));
        var podioFieldCaseNumber = Guard.Against.Null(podioFields.FirstOrDefault(x => x.Value.AppId == podioAppId && x.Value.Label == "CaseNumber"));
        Guard.Against.Null(podioFieldCaseNumber.Value);

        using (var scope = ServiceProvider.CreateScope())
        {
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            while (!stoppingToken.IsCancellationRequested)
            {
                var getQueueMessagesQuery = new GetQueueMessagesQuery(azureQueueName!);
                var azureQueueMessages = await mediator.SendRequest(getQueueMessagesQuery);

                if (azureQueueMessages.IsSuccess)
                {

                    foreach (var azureQueueMessage in azureQueueMessages.Value)
                    {
                        var deleteAzureQueueItemCommand = new DeleteQueueMessageCommand(azureQueueName, azureQueueMessage.Id, azureQueueMessage.PopReceipt);
                        await mediator.Send(deleteAzureQueueItemCommand);

                        if (string.IsNullOrEmpty(azureQueueMessage.Body))
                        {
                            _logger.LogError("Azure queue item body is empty. Expected a Podio item Id");
                            continue;
                        }

                        // Retrieve the Base64 encoded message from Azure Queue
                        string base64EncodedMessage = azureQueueMessage.Body.ToString();

                        // Decode the Base64 message back to a JSON string
                        string podioItemIdString = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(base64EncodedMessage));

                        if (!long.TryParse(podioItemIdString, out long podioItemId))
                        {
                            _logger.LogError("Could not parse the string '{string}' as a Podio Item Id", podioItemIdString);
                            continue;
                        }


                        // Get metadata from Podio
                        var getPodioItemQuery = new GetItemQuery(podioAppId, podioItemId);
                        var getPodioItemQueryResult = await mediator.SendRequest(getPodioItemQuery, stoppingToken);

                        if (!getPodioItemQueryResult.IsSuccess)
                        {
                            _logger.LogError("Could not get item {itemId} from Podio", podioItemId);
                            continue;
                        }

                        var caseNumber = getPodioItemQueryResult.Value.Fields.FirstOrDefault(x => x.Id == podioFieldCaseNumber.Key)?.Value?.FirstOrDefault();
                        if (string.IsNullOrEmpty(caseNumber))
                        {
                            _logger.LogError("Could not get case number field value from Podio Item {itemId}", podioItemId);
                            continue;
                        }

                        // Find Deskpro ticket by PodioItemId
                        var getTicketByPodioItemIdQuery = new GetTicketByPodioItemIdQuery(podioItemId);
                        var getTicketByPodioItemIdQueryResult = await mediator.SendRequest(getTicketByPodioItemIdQuery);

                        if (getTicketByPodioItemIdQueryResult.IsSuccess)
                        {
                            if (getTicketByPodioItemIdQueryResult.Value.Count() < 1)
                            {
                                _logger.LogError("0 Deskpro tickets found for PodioItemId {podioItemId}.", podioItemId);
                                continue;
                            }

                            if (getTicketByPodioItemIdQueryResult.Value.Count() > 1)
                            {
                                _logger.LogWarning("{count} Deskpro tickets found for PodioItemId {podioItemId}. Only processing the first.", getTicketByPodioItemIdQueryResult.Value.Count(), podioItemId);
                            }

                            var ticket = getTicketByPodioItemIdQueryResult.Value.FirstOrDefault();

                            if (ticket is null)
                            {
                                _logger.LogError("Ticket related to PodioItemId {id} not found in database", podioItemId);
                                continue;
                            }

                            // Get the case sharepoint folder name ("undermappenavn")
                            var caseSharepointFolderName = ticket.Cases?.FirstOrDefault(x => x.PodioItemId == podioItemId)?.SharepointFolderName;
                            if (caseSharepointFolderName == null)
                            {
                                _logger.LogError("Case related to PodioItemId {id}: SharepointFolderName is null or empty", podioItemId);
                            }

                            // Get data from Deskpro
                            var getDeskproTicketQuery = new GetDeskproTicketByIdQuery(ticket.DeskproId);
                            var getDeskproTicketQueryResult = await mediator.SendRequest(getDeskproTicketQuery);

                            if (getDeskproTicketQueryResult.IsSuccess)
                            {
                                (string Name, string Email) agent = new(string.Empty, string.Empty);

                                // Get Deskpro agent
                                if (getDeskproTicketQueryResult.Value.Agent is not null && getDeskproTicketQueryResult.Value.Agent.Id > 0)
                                {
                                    agent = await GetDeskproAgent(mediator, getDeskproTicketQueryResult.Value.Agent.Id);
                                }
                                else
                                {
                                    _logger.LogWarning($"Deskpro ticket {ticket.DeskproId} has no agents assigned");
                                }

                                // Post UiPath queue item
                                var queueItem = new UiPathQueueItem(
                                    deskproId: ticket.DeskproId,
                                    podioItemId: podioItemId,
                                    caseNumber: caseNumber,
                                    ticketSharepointFolderName: ticket.SharepointFolderName,
                                    caseSharepointFolderName: caseSharepointFolderName,
                                    agentName: agent.Name,
                                    title: getDeskproTicketQueryResult.Value.Subject,
                                    agentEmail: agent.Email);

                                var addUiPathQueueItemCommand = new AddQueueItemCommand(uiPathQueueName, podioItemId.ToString(), queueItem.Get());
                                await mediator.Send(addUiPathQueueItemCommand);
                            }
                            else
                            {
                                _logger.LogError($"Could not get data from Deskpro for ticket ID {ticket.DeskproId}");
                                continue;
                            }
                        }
                        else
                        {
                            _logger.LogError($"Could not get data from database for ticket by PodioItemId {podioItemId}");
                            continue;
                        }

                    }
                }
                else
                {
                    _logger.LogError($"Error getting queue items from Azure Queue '{azureQueueName}'");
                }

                await Task.Delay(TimeSpan.FromSeconds(delay), stoppingToken);
            }
        }
    }

    private async Task<(string Name, string Email)> GetDeskproAgent(IMediator mediator, int agentId)
    {
        var getAgentQuery = new GetDeskproPersonQuery(agentId);
        var getAgentResult = await mediator.SendRequest(getAgentQuery);

        if (getAgentResult.IsSuccess && getAgentResult.Value.IsAgent)
        {
            return (getAgentResult.Value.FullName, getAgentResult.Value.Email);
        }
        else
        {
            _logger.LogWarning($"Unable to get agent from Deskpro, agent id {agentId}");
        }

        return (string.Empty, string.Empty);
    }

    private record UiPathQueueItem(int deskproId, long podioItemId, string? caseNumber, string? ticketSharepointFolderName, string? caseSharepointFolderName, string title, string agentName, string agentEmail)
    {
        public object Get() => 
            new
            {
                SagsNummer = caseNumber,
                Email = agentEmail,
                Navn = agentName,
                PodioID = podioItemId,
                DeskproID = deskproId,
                Titel = title,
                Overmappenavn = ticketSharepointFolderName,
                Undermappenavn = caseSharepointFolderName
            };
    }
}
