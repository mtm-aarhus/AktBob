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

namespace AktBob.PodioHookProcessor.UseCases.MoveToSharepointTrigger;
internal class BackgroundWorker : BackgroundService
{
    private readonly ILogger<BackgroundWorker> _logger;
    private readonly IConfiguration _configuration;

    public BackgroundWorker(ILogger<BackgroundWorker> logger, IConfiguration configuration, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _configuration = configuration;
        ServiceProvider = serviceProvider;
    }

    public IServiceProvider ServiceProvider { get; }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var tenancyName = Guard.Against.NullOrEmpty(_configuration.GetValue<string>("UiPath:TenancyName"));
        var azureQueueName = Guard.Against.NullOrEmpty(_configuration.GetValue<string>($"MoveToSharepointTrigger:{tenancyName}:AzureQueueName"));
        var uiPathQueueName = Guard.Against.NullOrEmpty(_configuration.GetValue<string>($"MoveToSharepointTrigger:{tenancyName}:UiPathQueueName"));
        var delay = _configuration.GetValue<int?>("MoveToSharepointTrigger:WorkerIntervalSeconds") ?? 10;

        using (var scope = ServiceProvider.CreateScope())
        {
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

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

                    // Find Deskpro ticket from PodioItemId
                    var getTicketByPodioItemIdQuery = new GetTicketByPodioItemIdQuery(azureQueueItemDto.PodioItemId);
                    var getTicketByPodioItemIdQueryResult = await mediator.Send(getTicketByPodioItemIdQuery);

                    if (getTicketByPodioItemIdQueryResult.IsSuccess)
                    {

                        foreach (var ticket in getTicketByPodioItemIdQueryResult.Value)
                        {

                            var getDeskproTicketQuery = new GetDeskproTicketByIdQuery(ticket.Id);
                            var getDeskproTicketQueryResult = await mediator.Send(getDeskproTicketQuery);

                            if (getDeskproTicketQueryResult.IsSuccess)
                            {
                                var agentName = string.Empty;
                                var agentEmail = string.Empty;

                                // Skip if the Deskpro ticket has no assigned agent
                                if (getDeskproTicketQueryResult.Value.AgentId is not null || getDeskproTicketQueryResult.Value.AgentId > 0)
                                {
                                    // Get agent email address from Deskpro
                                    var getAgentQuery = new GetDeskproPersonQuery((int)getDeskproTicketQueryResult.Value.AgentId);
                                    var getAgentResult = await mediator.Send(getAgentQuery);


                                    if (getAgentResult.IsSuccess && getAgentResult.Value.IsAgent)
                                    {
                                        agentName = getAgentResult.Value.FullName;
                                        agentEmail = getAgentResult.Value.Email;
                                    }
                                }

                                var uiPathQueueItemContent = new
                                {
                                    SagsNummer = azureQueueItemDto.CaseNumber,
                                    Email = agentEmail,
                                    Navn = agentName,
                                    PodioID = azureQueueItemDto.PodioItemId,
                                    DeskproID = getDeskproTicketQueryResult.Value.Id,
                                    Titel = getDeskproTicketQueryResult.Value.Subject
                                };


                                // Post UiPath queue item
                                var addUiPathQueueItemCommand = new AddQueueItemCommand(uiPathQueueName, azureQueueItemDto.PodioItemId.ToString(), uiPathQueueItemContent);
                                await mediator.Send(addUiPathQueueItemCommand);
                            }

                        }
                    }

                    var deleteAzureQueueItemCommand = new DeleteQueueMessageCommand(azureQueueName, azureQueueMessage.Id, azureQueueMessage.PopReceipt);
                }
            }

            await Task.Delay(TimeSpan.FromSeconds(delay));
        }
    }
}
