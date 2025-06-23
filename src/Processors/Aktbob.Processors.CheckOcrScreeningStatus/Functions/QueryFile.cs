using AktBob.Database.Contracts;
using Aktbob.Processors.CheckOcrScreeningStatus.Jobs;
using AktBob.Shared;
using AktBob.Shared.Extensions;
using AktBob.Shared.ModuleClients.FilArkiv;
using Ardalis.GuardClauses;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Aktbob.Processors.CheckOcrScreeningStatus.Functions;

public class QueryFile(
    ILogger<QueryFile> logger,
    IFilArkivModuleClient filArkiv,
    IOcrScreeningStatusRepository repository,
    IConfiguration configuration,
    IMessageBus messageBus)
{
    [Function("query-file")]
    public async Task Run(
        [ServiceBusTrigger("%QueueNames:QueryFile%", Connection = "AzureServiceBus")]
        ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Message ID: {id} Body: {body} Content-Type: {contentType}", message.MessageId,  message.Body, message.ContentType);
        var job = MessageDeserializer.Deserialize<QueryFileJob>(message);

        var file = await repository.Get(job.FilArkivFileId);
        if (file == null)
        {
            logger.LogError("FilArkivFile {id} not found in database. Moving to DLQ.", job.FilArkivFileId);
            await messageActions.DeadLetterMessageAsync(message, deadLetterReason: $"FilArkivFile {job.FilArkivFileId} not found in database", cancellationToken: cancellationToken);
            return;
        }

        // The file has already been handled
        if (file.ProcessedAt is not null) return;

        // Get current status from FilArkiv
        var response = await filArkiv.GetFileProcessStatus(job.FilArkivFileId, cancellationToken);
        
        // Log and move to DLQ if file status cannot be retrived from FilArkiv
        if (response.IsError)
        {
            // Update repository
            file.ProcessedAt = DateTime.UtcNow;
            await repository.Update(file);
            
            // Log and move to DLQ
            response.LogResultErrors(logger);
            await messageActions.DeadLetterMessageAsync(message, deadLetterReason: $"Error getting FilArkivFile {job.FilArkivFileId} from FilArkiv", cancellationToken: cancellationToken);
            return;
        }

        // Reschedule iIf file processing is not finished yet
        if (response.Value.IsInQueue || response.Value.IsBeingProcessed)
        {
            // File not finished yet - reschedule
            await RescheduleFileStatusQuery(job);
            return;
        }

        // Finished - update cache
        logger.LogInformation("Case {caseId} File {fileId} finished", file.FilArkivCaseId, job.FilArkivFileId);
        file.ProcessedAt = DateTime.UtcNow;
        await repository.Update(file);
    }
    
    private async Task RescheduleFileStatusQuery(QueryFileJob job)
    {
        var queueName = Guard.Against.NullOrEmpty(configuration.GetValue<string>("QueueNames:QueryFile"));
        var offset = DateTimeOffset.UtcNow.AddSeconds(Math.Clamp(10 + Math.Pow(job.Count, 2), 0, 600));
        var count = job.Count + 1;
        
        logger.LogDebug("FilArkivFile {fileId} OCR-screening not finished yet. Retry in {delay}", job.FilArkivFileId, offset);
        await messageBus.ScheduleMessage(queueName, job with { Count = count }, offset);
    }
}