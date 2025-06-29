using AktBob.Database.Contracts;
using Aktbob.Processors.CheckOcrScreeningStatus.Contracts;
using Aktbob.Processors.CheckOcrScreeningStatus.Features.EmailNotification;
using Aktbob.Processors.CheckOcrScreeningStatus.Features.UpdatePodioItem;
using AktBob.Shared;
using Ardalis.GuardClauses;
using ErrorOr;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Aktbob.Processors.CheckOcrScreeningStatus.Features.NotificationDispatcher;

public class NotificationDispatcherHandler(
    ILogger<NotificationDispatcherHandler> logger,
    IConfiguration configuration,
    IOcrScreeningStatusRepository repository,
    IMessageBus messageBus)
{
    public async Task<ErrorOr<Success>> Run(DispatchNotificationJob job, CancellationToken cancellationToken)
    {
        // Exit early if no items are found in database
        if (!await repository.AnyByCaseId(job.FilArkivCaseId)) return Result.Success;
        
        // Reschedule if not all files are processed yet
        if (!await repository.AllFilesAreProcessed(job.FilArkivCaseId))
        {
            var offset = DateTimeOffset.UtcNow.AddMinutes(1); // TODO: get this from configuration
            var queueName = Guard.Against.NullOrEmpty(configuration.GetValue<string>("CheckOcrScreeningStatus:ServiceBusQueueNames:DispatchNotification"));
            logger.LogInformation("FilArkiv case {id}, PodioItemId {podioItemId}: not all files are finished OCR screening yet, rescheduling.", job.FilArkivCaseId, job.PodioItemId);
            
            await messageBus.ScheduleMessage(queueName, job, offset, cancellationToken);
            
            return Result.Success;
        }

        // All files are finished
        
        logger.LogInformation("FilArkiv case {id}, PodioItemId {podioItemId}: all files finished OCR screening", job.FilArkivCaseId, job.PodioItemId);

        // Remove case from database
        //await repository.RemoveByCaseId(job.FilArkivCaseId); // TODO: do this in a separate clean up job that runs once a day or similar

        // Dispatch notification jobs
        await Task.WhenAll([
            EnqueuePodioFieldUpdate(job.PodioItemId, job.FilArkivCaseId, cancellationToken),
            EnqueueEmailNotification(job.PodioItemId, job.FilArkivCaseId, cancellationToken),
            EnqueuePodioNotification(job.PodioItemId, cancellationToken)
        ]);
        
        return Result.Success;
    }
    
    private async Task EnqueuePodioFieldUpdate(long podioItemId, Guid filArkivCaseId, CancellationToken cancellationToken)
    {
        if (Settings.ShouldPodioItemBeUpdatedImmediately(configuration)) return; // Do nothing if the Podio item was already updated immediately after registering files
     
        var updatePodioItemQueueName = Guard.Against.NullOrEmpty(configuration.GetValue<string>("CheckOcrScreeningStatus:ServiceBusQueueNames:UpdatePodioItem"));
        await messageBus.SendMessage(updatePodioItemQueueName, new UpdatePodioItemJob(podioItemId, filArkivCaseId), cancellationToken);
    }
    
    private async Task EnqueueEmailNotification(long podioItemId, Guid filArkivCseId, CancellationToken cancellationToken)
    {
        var queueName = Guard.Against.NullOrEmpty(configuration.GetValue<string>("CheckOcrScreeningStatus:ServiceBusQueueNames:EmailNotification"));
        await messageBus.SendMessage(queueName, new EmailNotificationJob(podioItemId, filArkivCseId), cancellationToken);
    }

    private async Task EnqueuePodioNotification(long podioItemId, CancellationToken cancellationToken)
    {
        var queueName = Guard.Against.NullOrEmpty(configuration.GetValue<string>("CheckOcrScreeningStatus:ServiceBusQueueNames:PodioNotification"));
        await messageBus.SendMessage(queueName, new PodioNotificationJob(podioItemId), cancellationToken);
    }
}