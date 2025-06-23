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
}