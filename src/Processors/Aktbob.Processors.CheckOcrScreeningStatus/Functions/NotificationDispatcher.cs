using AktBob.Database.Contracts;
using Aktbob.Processors.CheckOcrScreeningStatus.Jobs;
using AktBob.Shared;
using Ardalis.GuardClauses;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Aktbob.Processors.CheckOcrScreeningStatus.Functions;

public class NotificationDispatcher(
    ILogger<NotificationDispatcher> logger,
    IConfiguration configuration,
    IOcrScreeningStatusRepository repository,
    IMessageBus messageBus)
{
    [Function("notification-dispatcher")]
    public async Task Run(
        [ServiceBusTrigger("%QueueNames:DispatchNotification%", Connection = "AzureServiceBus")]
        ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Message ID: {id} Body: {body} Content-Type: {contentType}", message.MessageId,  message.Body, message.ContentType);
        var job = MessageDeserializer.Deserialize<DispatchNotificationJob>(message);

        // Exit early if no items are found in database ( = notification has already been handled)
        if (!await repository.AnyByCaseId(job.FilArkivCaseId)) return;
        
        // Reschedule if not all files are processed yet
        if (!await repository.AllFilesAreProcessed(job.FilArkivCaseId))
        {
            var offset = DateTimeOffset.UtcNow.AddMinutes(2); // TODO: get this from configuration
            var queueName = Guard.Against.NullOrEmpty(configuration.GetValue<string>("QueueNames:DispatchNotification"));
            logger.LogInformation("FilArkiv case {id}, PodioItemId {podioItemId}: not all files are finished OCR screening yet, rescheduling.", job.FilArkivCaseId, job.PodioItemId);
            
            await messageBus.ScheduleMessage(queueName, job, offset, cancellationToken);
            
            return;
        }

        // All files are finished
        
        logger.LogInformation("FilArkiv case {id}, PodioItemId {podioItemId}: all files finished OCR screening", job.FilArkivCaseId, job.PodioItemId);

        // Remove case from database
        await repository.RemoveByCaseId(job.FilArkivCaseId);

        // Dispatch notification jobs
        await Task.WhenAll([
            EnqueuePodioFieldUpdate(job.PodioItemId, job.FilArkivCaseId, cancellationToken),
            EnqueueEmailNotification(job.PodioItemId, job.FilArkivCaseId, cancellationToken),
            EnqueuePodioNotification(job.PodioItemId, cancellationToken)
        ]);
        
    }
    
    private async Task EnqueuePodioFieldUpdate(long podioItemId, Guid filArkivCaseId, CancellationToken cancellationToken)
    {
        if (Settings.ShouldPodioItemBeUpdatedImmediately(configuration)) return; // Do nothing if the Podio item was already updated immediately after registering files
     
        var updatePodioItemQueueName = Guard.Against.NullOrEmpty(configuration.GetValue<string>("QueueNames:UpdatePodioItem"));
        await messageBus.SendMessage(updatePodioItemQueueName, new UpdatePodioItemJob(podioItemId, filArkivCaseId), cancellationToken);
    }
    
    private async Task EnqueueEmailNotification(long podioItemId, Guid filArkivCseId, CancellationToken cancellationToken)
    {
        var queueName = Guard.Against.NullOrEmpty(configuration.GetValue<string>("QueueNames:EmailNotification"));
        await messageBus.SendMessage(queueName, new EmailNotificationJob(podioItemId, filArkivCseId), cancellationToken);
    }

    private async Task EnqueuePodioNotification(long podioItemId, CancellationToken cancellationToken)
    {
        var queueName = Guard.Against.NullOrEmpty(configuration.GetValue<string>("QueueNames:PodioNotification"));
        await messageBus.SendMessage(queueName, new PodioNotificationJob(podioItemId), cancellationToken);
    }
}