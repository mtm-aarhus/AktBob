using AktBob.Database.Contracts;
using AktBob.Shared;
using AktBob.Shared.ModuleClients.OpenOrchestratorModule;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Ardalis.GuardClauses;
using Microsoft.Extensions.Configuration;

namespace Aktbob.Processors.Cleanup;

public class CreateCleanupFilArkivQueueItem(
    ILogger<CreateCleanupFilArkivQueueItem> logger,
    IConfiguration configuration,
    ITicketRepository ticketRepository,
    IOpenOrchestratorModuleClient openOrchestrator)
{
    [Function("create-open-orchestrator-queue-item-cleanup-filarkiv")]
    public async Task Run(
        [ServiceBusTrigger("%ServiceBusQueueNameCleanUpFilArkiv%", Connection = "AzureServiceBus")]
        ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Message ID: {id} Body: {body} Content-Type: {contentType}", message.MessageId,  message.Body, message.ContentType);
        var job = MessageDeserializer.Deserialize<CreateCleanupFilArkivQueueItemJob>(message);
        
        // Variables
        var openOrchestratorQueueName = Guard.Against.NullOrEmpty(configuration.GetValue<string>("OpenOrchestrator:CleanUpFilArkivQueueName"));
        
        // Get FilArkiv caseId from database
        var tickets = await ticketRepository.GetAll(deskproId: job.TicketId, null, null);
        if (tickets.Count == 0)
        {
            logger.LogError("Deskpro ticket {ticketId} not found in database. Moving message to DLQ.", job.TicketId);
            await messageActions.DeadLetterMessageAsync(message, deadLetterReason: $"Deskpro ticket {job.TicketId} not found in database", cancellationToken: cancellationToken);
            return;
        }

        foreach (var ticket in tickets)
        {
            // Skip if no cases on the ticket (nothing to clean up)
            if (ticket.Cases.Count == 0)
            {
                logger.LogWarning("No cases found in database for Deskpro ticket {id}", ticket.DeskproId);
                continue;
            }

            foreach (var @case in ticket.Cases)
            {
                // Skip if case has no FilArkiv case ID (nothing to clean up)
                if (@case.FilArkivCaseId is null)
                {
                    logger.LogWarning("FilArkivCaseId is null for case {caseId} DeskproTicketId {id}", @case.CaseNumber, job.TicketId);
                    continue;
                }

                // Create OpenOrchestrator queue item
                var payload = new
                {
                    @case.FilArkivCaseId
                };
                
                await openOrchestrator.AddQueueItem(openOrchestratorQueueName, $"Deskpro {job.TicketId} {@case.CaseNumber}", payload, cancellationToken);
            }
        }
    }
}