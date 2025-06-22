using AktBob.Database.Contracts;
using AktBob.Shared;
using AktBob.Shared.ModuleClients.OpenOrchestratorModule;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Ardalis.GuardClauses;
using Microsoft.Extensions.Configuration;

namespace Aktbob.Processors.Cleanup;

public class CreateCleanupSharepointQueueItem(
    ILogger<CreateCleanupSharepointQueueItem> logger,
    IConfiguration configuration,
    ITicketRepository ticketRepository,
    IOpenOrchestratorModuleClient openOrchestrator)
{
    [Function("create-open-orchestrator-queue-item-cleanup-sharepoint")]
    public async Task Run(
        [ServiceBusTrigger("%ServiceBusQueueNameCleanUpSharepoint%", Connection = "AzureServiceBus")]
        ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Message ID: {id} Body: {body} Content-Type: {contentType}", message.MessageId,  message.Body, message.ContentType);
        var job = MessageDeserializer.Deserialize<CreateCleanupSharepointQueueItemJob>(message);
        
        // Variables
        var openOrchestratorQueueName = Guard.Against.NullOrEmpty(configuration.GetValue<string>("OpenOrchestrator:CleanUpSharepointQueueName"));

        // Get Sharepoint folder name from database
        var ticket = await ticketRepository.GetByDeskproTicketId(job.TicketId);
        if (ticket == null)
        {
            logger.LogError("Deskpro ticket {ticketId} not found in database. Moving message to DLQ.", job.TicketId);
            await messageActions.DeadLetterMessageAsync(message, deadLetterReason: $"Deskpro ticket {job.TicketId} not found in database", cancellationToken: cancellationToken);
            return;
        }

        if (string.IsNullOrWhiteSpace(ticket.SharepointFolderName))
        {
            logger.LogError("No Sharepoint folder name registered for Deskpro ticket {ticketId}. Moving message to DLQ.", job.TicketId);
            await messageActions.DeadLetterMessageAsync(message, deadLetterReason: $"No Sharepoint folder name registered for Deskpro ticket {job.TicketId}.", cancellationToken: cancellationToken);
            return;
        }

        // Create OpenOrchestrator queue item
        var payload = new
        {
            SharepointMappeNavn = ticket.SharepointFolderName
        };

        await openOrchestrator.AddQueueItem(openOrchestratorQueueName, $"Deskpro {job.TicketId}", payload, cancellationToken);
    }
}