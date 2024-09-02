using AAK.Podio;
using AktBob.DatabaseAPI.Contracts;
using AktBob.Deskpro.Contracts;
using AktBob.Queue.Contracts;
using AktBob.UiPath.Contracts;
using Ardalis.GuardClauses;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;

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
                var azureQueueMessages = await mediator.Send(getQueueMessagesQuery);

                if (azureQueueMessages.IsSuccess)
                {

                    foreach (var azureQueueMessage in azureQueueMessages.Value)
                    {
                        if (string.IsNullOrEmpty(azureQueueMessage.Body))
                        {
                            _logger.LogError("Azure queue item body is empty. Expected a Podio item Id");
                            await DeleteQueueItem(azureQueueName, mediator, azureQueueMessage);
                            continue;
                        }

                        var messageContent = JsonSerializer.Deserialize<string>(azureQueueMessage.Body);

                        if (!long.TryParse(messageContent, out long podioItemId))
                        {
                            _logger.LogError("Could not parse Azure queue item body as a 'long' data type. Body content: '{body}'", azureQueueMessage.Body);
                            await DeleteQueueItem(azureQueueName, mediator, azureQueueMessage);
                            continue;
                        }


                        // Get metadata from Podio
                        var podioItemQueryResult = await _podio.GetItem(podioAppId, podioItemId, stoppingToken);
                        if (!podioItemQueryResult.IsSuccess)
                        {
                            _logger.LogError("Could not get item {itemId} from Podio", podioItemId);
                            await DeleteQueueItem(azureQueueName, mediator, azureQueueMessage);
                            continue;
                        }

                        var caseNumber = podioItemQueryResult.Value.Fields.FirstOrDefault(x => x.Id == podioFieldCaseNumber.Key)?.Value?.FirstOrDefault();
                        if (string.IsNullOrEmpty(caseNumber))
                        {
                            _logger.LogError("Could not get case number field value from Podio Item {itemId}", podioItemId);
                            await DeleteQueueItem(azureQueueName, mediator, azureQueueMessage);
                            continue;
                        }

                        // Find Deskpro ticket from PodioItemId
                        var getTicketByPodioItemIdQuery = new GetTicketByPodioItemIdQuery(podioItemId);
                        var getTicketByPodioItemIdQueryResult = await mediator.Send(getTicketByPodioItemIdQuery);

                        if (getTicketByPodioItemIdQueryResult.IsSuccess)
                        {
                            if (getTicketByPodioItemIdQueryResult.Value.Count() > 1)
                            {
                                _logger.LogWarning("{count} Deskpro ticket found for PodioItemId {podioItemId}. Only processing the first.", getTicketByPodioItemIdQueryResult.Value.Count(), podioItemId);
                            }

                            var ticket = getTicketByPodioItemIdQueryResult.Value.FirstOrDefault();

                            if (ticket is null)
                            {
                                _logger.LogError("Ticket related to PodioItemId {id} not found in database", podioItemId);
                                await DeleteQueueItem(azureQueueName, mediator, azureQueueMessage);
                                continue;
                            }

                            // Get data from Deskpro
                            var getDeskproTicketQuery = new GetDeskproTicketByIdQuery(ticket.DeskproId);
                            var getDeskproTicketQueryResult = await mediator.Send(getDeskproTicketQuery);

                            if (getDeskproTicketQueryResult.IsSuccess)
                            {
                                var agentName = string.Empty;
                                var agentEmail = string.Empty;

                                // Skip if the Deskpro ticket has no assigned agent
                                if (getDeskproTicketQueryResult.Value.Agent is not null && getDeskproTicketQueryResult.Value.Agent.Id > 0)
                                {
                                    // Get agent email address from Deskpro
                                    var getAgentQuery = new GetDeskproPersonQuery(getDeskproTicketQueryResult.Value.Agent.Id!);
                                    var getAgentResult = await mediator.Send(getAgentQuery);

                                    if (getAgentResult.IsSuccess && getAgentResult.Value.IsAgent)
                                    {
                                        agentName = getAgentResult.Value.FullName;
                                        agentEmail = getAgentResult.Value.Email;
                                    }
                                }

                                var uiPathQueueItemContent = new
                                {
                                    SagsNummer = caseNumber,
                                    Email = agentEmail,
                                    Navn = agentName,
                                    PodioID = podioItemId,
                                    DeskproID = getDeskproTicketQueryResult.Value.Id,
                                    Titel = getDeskproTicketQueryResult.Value.Subject
                                };


                                // Post UiPath queue item
                                var addUiPathQueueItemCommand = new AddQueueItemCommand(uiPathQueueName, podioItemId.ToString(), uiPathQueueItemContent);
                                await mediator.Send(addUiPathQueueItemCommand);
                            }

                        }

                        await DeleteQueueItem(azureQueueName, mediator, azureQueueMessage);
                    }
                }

                await Task.Delay(TimeSpan.FromSeconds(delay));
            }
        }
    }

    private static async Task DeleteQueueItem(string azureQueueName, IMediator mediator, QueueMessageDto? azureQueueMessage)
    {
        var deleteAzureQueueItemCommand = new DeleteQueueMessageCommand(azureQueueName, azureQueueMessage.Id, azureQueueMessage.PopReceipt);
        await mediator.Send(deleteAzureQueueItemCommand);
    }
}
