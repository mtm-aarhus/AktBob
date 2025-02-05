using System.Text;
using AktBob.Database.Contracts;
using AktBob.Deskpro.Contracts;
using AktBob.OpenOrchestrator.Contracts;
using AktBob.Queue.Contracts;
using AktBob.UiPath.Contracts;
using Ardalis.GuardClauses;
using MassTransit;
using MassTransit.Mediator;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AktBob.PodioHookProcessor.UseCases.JournalizeEverythingTrigger;
internal class BackgroundWorker : BackgroundService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<BackgroundWorker> _logger;
    private readonly string _configurationObjectName = "JournalizeEverythingTrigger";

    public IServiceProvider ServiceProvider { get; }

    public BackgroundWorker(IServiceProvider serviceProvider, IConfiguration configuration, ILogger<BackgroundWorker> logger)
    {
        ServiceProvider = serviceProvider;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Background service variables
        var azureQueueName = Guard.Against.NullOrEmpty(_configuration.GetValue<string>($"{_configurationObjectName}:AzureQueueName"));
        var delay = _configuration.GetValue<int?>($"{_configurationObjectName}:WorkerIntervalSeconds") ?? 10;

        // UiPath variables
        var uiPathTenancyName = Guard.Against.NullOrEmpty(_configuration.GetValue<string>("UiPath:TenancyName"));
        var uiPathQueueName = Guard.Against.NullOrEmpty(_configuration.GetValue<string>($"{_configurationObjectName}:UiPathQueueName:{uiPathTenancyName}"));

        // OpenOrchestrator variables
        var openOrchestratorQueueName = Guard.Against.NullOrEmpty(_configuration.GetValue<string>($"{_configurationObjectName}:OpenOrchestratorQueueName"));
        var useOpenOrchestrator = _configuration.GetValue<bool>($"{_configurationObjectName}:UseOpenOrchestrator");
        
        using (var scope = ServiceProvider.CreateScope())
        {
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            while (!stoppingToken.IsCancellationRequested)
            {
                var getAzureQueueMessagesQuery = new GetQueueMessagesQuery(azureQueueName);
                var getAzureQueueMessagesResult = await mediator.SendRequest(getAzureQueueMessagesQuery);

                if (getAzureQueueMessagesResult.IsSuccess)
                {
                    foreach (var azureQueueMessage in getAzureQueueMessagesResult.Value)
                    {
                        // Delete queue item from Azure queue
                        var deleteAzureQueueItemCommand = new DeleteQueueMessageCommand(azureQueueName, azureQueueMessage.Id, azureQueueMessage.PopReceipt);
                        await mediator.Send(deleteAzureQueueItemCommand);

                        if (string.IsNullOrEmpty(azureQueueMessage.Body))
                        {
                            _logger.LogError("Azure queue item body is empty. Expected a Deskpro ticket Id");
                            continue;
                        }



                        // GET CONTENT OF QUEUE ITEM

                        // Retrieve the Base64 encoded message from Azure Queue
                        string base64EncodedMessage = azureQueueMessage.Body.ToString();

                        // Decode the Base64 message back to a JSON string
                        string deskproIdString = Encoding.UTF8.GetString(Convert.FromBase64String(base64EncodedMessage));

                        if (!int.TryParse(deskproIdString, out int deskproId))
                        {
                            _logger.LogError("Could not parse the string '{string}' as a Deskpro ticket Id", deskproIdString);
                            continue;
                        }




                        // GET DATA FROM API DATABASE
                        var getDataFromApiDatabaseQuery = new GetTicketsQuery(deskproId, null, null);
                        var getDataFromApiDatabaseResult = await mediator.SendRequest(getDataFromApiDatabaseQuery);

                        if (getDataFromApiDatabaseResult.IsSuccess)
                        {
                            if (getDataFromApiDatabaseResult.Value.Count() < 1)
                            {
                                _logger.LogError("0 Deskpro tickets found for id {id}.", deskproId);
                                continue;
                            }

                            if (getDataFromApiDatabaseResult.Value.Count() > 1)
                            {
                                _logger.LogWarning("{count} Deskpro tickets found for id {id}. Only processing the first.", getDataFromApiDatabaseResult.Value.Count(), deskproId);
                            }

                            var ticket = getDataFromApiDatabaseResult.Value.FirstOrDefault();

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



                            // GET DATA FROM DESKPRO
                            var getDeskproTicketQuery = new GetDeskproTicketByIdQuery(ticket.DeskproId);
                            var getDeskproTicketQueryResult = await mediator.SendRequest(getDeskproTicketQuery);

                            if (getDeskproTicketQueryResult.IsSuccess)
                            {
                                var agentName = string.Empty;
                                var agentEmail = string.Empty;

                                // Skip if the Deskpro ticket has no assigned agent
                                if (getDeskproTicketQueryResult.Value.Agent is not null && getDeskproTicketQueryResult.Value.Agent.Id > 0)
                                {
                                    // Get agent email address from Deskpro
                                    var getAgentQuery = new GetDeskproPersonQuery(getDeskproTicketQueryResult.Value.Agent.Id!);
                                    var getAgentResult = await mediator.SendRequest(getAgentQuery);

                                    if (getAgentResult.IsSuccess && getAgentResult.Value.IsAgent)
                                    {
                                        agentName = getAgentResult.Value.FullName;
                                        agentEmail = getAgentResult.Value.Email;
                                    }
                                }



                                // CREATE QUEUE ITEM FOR UIPATH/OPENORCHESTRATOR

                                if (useOpenOrchestrator)
                                {
                                    // Create OpenOrchestrator queue item
                                    var data = new
                                    {
                                        Aktindsigtssag = ticket.CaseNumber,
                                        Email = agentEmail,
                                        Navn = agentName,
                                        DeskproID = deskproId,
                                        Overmappenavn = ticket.SharepointFolderName
                                    };

                                    var command = new CreateQueueItemCommand(openOrchestratorQueueName, data, $"Deskpro ID {deskproId}");
                                    await mediator.Send(command, stoppingToken);
                                }
                                else
                                {
                                    // Create UiPath queue item
                                    var uiPathQueueItemContent = new
                                    {
                                        Aktindsigtssag = ticket.CaseNumber,
                                        Email = agentEmail,
                                        Navn = agentName,
                                        DeskproID = deskproId,
                                        Overmappenavn = ticket.SharepointFolderName
                                    };

                                    var addUiPathQueueItemCommand = new AddQueueItemCommand(uiPathQueueName, $"DeskproId {deskproId.ToString()}", uiPathQueueItemContent);
                                    await mediator.Send(addUiPathQueueItemCommand);
                                }
                            }

                        }
                    }
                }

                await Task.Delay(TimeSpan.FromSeconds(delay), stoppingToken);
            }
        }
    }
}
