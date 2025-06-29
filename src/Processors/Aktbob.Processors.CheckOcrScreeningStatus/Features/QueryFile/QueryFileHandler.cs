using AktBob.Database.Contracts;
using Aktbob.Modules.FilArkiv.Features.GetFileProcessStatus;
using AktBob.Shared;
using AktBob.Shared.Extensions;
using Ardalis.GuardClauses;
using ErrorOr;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Aktbob.Processors.CheckOcrScreeningStatus.Features.QueryFile;

internal class QueryFileHandler(
    ILogger<QueryFileHandler> logger,
    IGetFileProcessStatusHandler filArkivGetFileProcessStatusHandler,
    IOcrScreeningStatusRepository repository,
    IConfiguration configuration,
    IMessageBus messageBus)
{
    public async Task<ErrorOr<Success>> Run(QueryFileJob job, CancellationToken cancellationToken)
    {
        var file = await repository.Get(job.FilArkivFileId);
        if (file == null)
        {
            logger.LogError("FilArkivFile {id} not found in database. Moving to DLQ.", job.FilArkivFileId);
            return Error.Failure($"FilArkivFile {job.FilArkivFileId} not found in database");
        }

        // The file has already been handled
        if (file.ProcessedAt is not null) return Result.Success;

        // Get current status from FilArkiv
        var response = await filArkivGetFileProcessStatusHandler.Handle(job.FilArkivFileId, cancellationToken);
        
        // Log and move to DLQ if file status cannot be retrived from FilArkiv
        if (response.IsError)
        {
            // Update repository
            file.ProcessedAt = DateTime.UtcNow;
            await repository.Update(file);
            
            // Log and move to DLQ
            response.LogResultErrors(logger);
            return Error.Failure($"Error getting FilArkivFile {job.FilArkivFileId} from FilArkiv");
        }

        // Reschedule iIf file processing is not finished yet
        if (response.Value.IsInQueue || response.Value.IsBeingProcessed)
        {
            // File not finished yet - reschedule
            await RescheduleFileStatusQuery(job);
            return Result.Success;
        }

        // Finished - update cache
        logger.LogInformation("Case {caseId} File {fileId} finished", file.FilArkivCaseId, job.FilArkivFileId);
        file.ProcessedAt = DateTime.UtcNow;
        await repository.Update(file);

        return Result.Success;
    }
    
    private async Task RescheduleFileStatusQuery(QueryFileJob job)
    {
        var queueName = Guard.Against.NullOrEmpty(configuration.GetValue<string>("CheckOcrScreeningStatus:ServiceBusQueueNames:QueryFile"));
        var offset = DateTimeOffset.UtcNow.AddSeconds(Math.Clamp(10 + Math.Pow(job.Count, 2), 0, 600));
        var count = job.Count + 1;
        
        logger.LogDebug("FilArkivFile {fileId} OCR-screening not finished yet. Retry in {delay}", job.FilArkivFileId, offset);
        await messageBus.ScheduleMessage(queueName, job with { Count = count }, offset);
    }
}