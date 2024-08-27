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

namespace AktBob.Aktliste;
internal class Worker : BackgroundService
{
    private ILogger<Worker> _logger;
    private IConfiguration _configuration;

    public IServiceProvider ServiceProvider { get; }

    public Worker(ILogger<Worker> logger, IConfiguration configuration, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _configuration = configuration;
        ServiceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var azureQueueName = Guard.Against.NullOrEmpty(_configuration.GetValue<string>("AktlisteModule:AzureQueueName"));
        var uiPathQueueName = Guard.Against.NullOrEmpty(_configuration.GetValue<string>("AktlisteModule:UiPathQueueName"));
        var delay = _configuration.GetValue<int?>("AktlisteModule:WorkerIntervalSeconds") ?? 10;

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
                        var azureQueueItemDto = JsonSerializer.Deserialize<AzureQueueItemDto>(azureQueueMessage.Body);

                        if (azureQueueItemDto == null)
                        {
                            _logger.LogError("Azure queue item body does not match type of '{type}'", typeof(AzureQueueItemDto));
                            continue;
                        }

                        // Get data from Deskpro

                        // Find Deskpro ticket from PodioItemId
                        var ticketFields = _configuration.GetSection("Deskpro:PodioItemIdFields").Get<int[]>();

                        // Get Deskpro tickets by searching the specified custom fields for the PodioItemId 
                        var getTicketsQuery = new GetDeskproTicketsByFieldSearchQuery(ticketFields!, azureQueueItemDto.PodioItemId.ToString());
                        var deskproTickets = await mediator.Send(getTicketsQuery);

                        foreach (var deskproTicket in deskproTickets.Value)
                        {
                            // Skip if the Deskpro ticket has no assigned agent
                            if (deskproTicket.AgentId is null || deskproTicket.AgentId == 0)
                            {
                                continue;
                            }

                            // Get agent email address from Deskpro
                            var getAgentQuery = new GetDeskproPersonQuery((int)deskproTicket.AgentId);
                            var getAgentResult = await mediator.Send(getAgentQuery);

                            var agentName = string.Empty;
                            var agentEmail = string.Empty;

                            if (getAgentResult.IsSuccess && getAgentResult.Value.IsAgent)
                            {
                                agentName = getAgentResult.Value.FullName;
                                agentEmail = getAgentResult.Value.Email;
                            }

                            var uiPathQueueItemContent = new
                            {
                                SagsNummer = azureQueueItemDto.CaseNumber,
                                Email = agentEmail,
                                Navn = agentName,
                                PodioID = azureQueueItemDto.PodioItemId,
                                DeskproID = deskproTicket.Id,
                                Titel = deskproTicket.Subject
                            };

                            var addUiPathQueueItemCommand = new AddQueueItemCommand(uiPathQueueName, azureQueueItemDto.PodioItemId.ToString(), uiPathQueueItemContent);
                            await mediator.Send(addUiPathQueueItemCommand);
                        }

                        var deleteAzureQueueItemCommand = new DeleteQueueMessageCommand(azureQueueName, azureQueueMessage.Id, azureQueueMessage.PopReceipt);
                    }
                }

                await Task.Delay(TimeSpan.FromSeconds(delay));
            }
        }
    }
}
