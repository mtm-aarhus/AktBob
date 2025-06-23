using AktBob.Database.Contracts;
using AktBob.Database.Entities;
using AktBob.Database.Repositories;
using Microsoft.Extensions.Logging;

namespace AktBob.Database.Decorators;

internal class OcrScreeningStatusLoggingDecorator(
    IOcrScreeningStatusRepository next,
    ILogger<OcrScreeningStatusRepository> logger)
    : IOcrScreeningStatusRepository
{
    public async Task<bool> Add(OcrScreeningStatus ocrScreeningStatus)
    {
        logger.LogInformation("Adding to database {item}", ocrScreeningStatus);
        var success = await next.Add(ocrScreeningStatus);

        if (!success)
        {
            logger.LogDebug("{name}: No rows were affected when trying to add {item}", nameof(Add), ocrScreeningStatus);
        }

        return success;
    }

    public async Task<bool> Update(OcrScreeningStatus ocrScreeningStatus)
    {
        logger.LogInformation("Updating database OcrScreeningStatus {item}", ocrScreeningStatus);
        
        var success = await next.Update(ocrScreeningStatus);

        if (!success)
        {
            logger.LogDebug("{name}: No rows were affected when trying to update {item}", nameof(Update), ocrScreeningStatus);
        }

        return success;
    }

    public async Task<OcrScreeningStatus?> Get(Guid filArkivFileId)
    {
        logger.LogInformation("Getting OcrScreeningStatus by FilArkivFileId {id}", filArkivFileId);
        
        var item = await next.Get(filArkivFileId);
        if (item == null)
        {
            logger.LogDebug("OcrScreeningStatus by FilArkivFileId {id} not found", filArkivFileId);
        }

        return item;
    }

    public async Task RemoveByCaseId(Guid filArkivCaseId)
    {
        logger.LogInformation("Removing all OcrScreeningStatusses by FilArkivCaseId {id}", filArkivCaseId);
        await next.RemoveByCaseId(filArkivCaseId);
    }

    public async Task<bool> AnyByCaseId(Guid filarkivCaseId)
    {
        logger.LogInformation("Checking if any OcrScreeningStatus exists by FilArkivCaseId {id}", filarkivCaseId);
        return await next.AnyByCaseId(filarkivCaseId);
    }

    public async Task<bool> AllFilesAreProcessed(Guid filarkivCaseId)
    {
        logger.LogInformation("Checking if all OcrScreeningStatus is processed by FilArkivCaseId {id}", filarkivCaseId);
        return await next.AllFilesAreProcessed(filarkivCaseId);
    }
}