using System.Text;
using AktBob.DatabaseAPI.Contracts.Queries;
using AktBob.Deskpro.Contracts;
using AktBob.Queue.Contracts;
using AktBob.UiPath.Contracts;
using Ardalis.GuardClauses;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AktBob.PodioHookProcessor.UseCases.JournalizeEverythingTrigger;
internal class BackgroundWorker : BackgroundService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<BackgroundWorker> _logger;

    public IServiceProvider ServiceProvider { get; }

    public BackgroundWorker(IServiceProvider serviceProvider, IConfiguration configuration, ILogger<BackgroundWorker> logger)
    {
        ServiceProvider = serviceProvider;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var tenancyName = Guard.Against.NullOrEmpty(_configuration.GetValue<string>("UiPath:TenancyName"));
        var azureQueueName = Guard.Against.NullOrEmpty(_configuration.GetValue<string>($"JournalizeEverythingTrigger:AzureQueueName"));
        var uiPathQueueName = Guard.Against.NullOrEmpty(_configuration.GetValue<string>($"JournalizeEverythingTrigger:UiPathQueueName:{tenancyName}"));
        var delay = _configuration.GetValue<int?>("JournalizeEverythingTrigger:WorkerIntervalSeconds") ?? 10;

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
                        var deleteAzureQueueItemCommand = new DeleteQueueMessageCommand(azureQueueName, azureQueueMessage.Id, azureQueueMessage.PopReceipt);
                        await mediator.Send(deleteAzureQueueItemCommand);

                        if (string.IsNullOrEmpty(azureQueueMessage.Body))
                        {
                            _logger.LogError("Azure queue item body is empty. Expected a Deskpro ticket Id");
                            continue;
                        }

                        // Retrieve the Base64 encoded message from Azure Queue
                        string base64EncodedMessage = azureQueueMessage.Body.ToString();

                        // Decode the Base64 message back to a JSON string
                        string deskproIdString = Encoding.UTF8.GetString(Convert.FromBase64String(base64EncodedMessage));

                        if (!int.TryParse(deskproIdString, out int deskproId))
                        {
                            _logger.LogError("Could not parse the string '{string}' as a Deskpro ticket Id", deskproIdString);
                            continue;
                        }


                        
                        var getTicketByPodioItemIdQuery = new GetTicketByDeskproIdQuery(deskproId);
                        var getTicketByPodioItemIdQueryResult = await mediator.Send(getTicketByPodioItemIdQuery);

                        if (getTicketByPodioItemIdQueryResult.IsSuccess)
                        {
                            if (getTicketByPodioItemIdQueryResult.Value.Count() < 1)
                            {
                                _logger.LogError("0 Deskpro tickets found for id {id}.", deskproId);
                                continue;
                            }

                            if (getTicketByPodioItemIdQueryResult.Value.Count() > 1)
                            {
                                _logger.LogWarning("{count} Deskpro tickets found for id {id}. Only processing the first.", getTicketByPodioItemIdQueryResult.Value.Count(), deskproId);
                            }

                            var ticket = getTicketByPodioItemIdQueryResult.Value.FirstOrDefault();

                            if (ticket is null)
                            {
                                _logger.LogError("Ticket related to Deskpro Id {id} not found in database", deskproId);
                                continue;
                            }

                            if (string.IsNullOrEmpty(ticket.CaseNumber))
                            {
                                _logger.LogError("GO Aktindsigtssagsnummer not registered for Deskpro Id {id}", deskproId);
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
                                    Aktindsigtssag = ticket.CaseNumber,
                                    Email = agentEmail,
                                    Navn = agentName,
                                    DeskproID = deskproId,
                                    OvermappeNavnAktindsigter = ticket.FolderNameAktindsigter,
                                    OvermappenavnDokumentlister = ticket.FolderNameDocumentLists
                                };


                                // Post UiPath queue item
                                var addUiPathQueueItemCommand = new AddQueueItemCommand(uiPathQueueName, $"DeskproId {deskproId.ToString()}", uiPathQueueItemContent);
                                await mediator.Send(addUiPathQueueItemCommand);
                            }

                        }

                    }
                }

                await Task.Delay(TimeSpan.FromSeconds(delay), stoppingToken);
            }
        }
    }
}
