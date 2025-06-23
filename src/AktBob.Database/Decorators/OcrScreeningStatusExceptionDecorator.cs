using AktBob.Database.Contracts;
using AktBob.Database.Entities;
using AktBob.Database.Repositories;
using Microsoft.Extensions.Logging;

namespace AktBob.Database.Decorators;

internal class OcrScreeningStatusExceptionDecorator(
    IOcrScreeningStatusRepository next,
    ILogger<OcrScreeningStatusRepository> logger)
    : IOcrScreeningStatusRepository
{
    public async Task<bool> Add(OcrScreeningStatus ocrScreeningStatus)
    {
        try
        {
            return await next.Add(ocrScreeningStatus);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in {name}", nameof(Add));
            throw;
        }
    }

    public async Task<bool> Update(OcrScreeningStatus ocrScreeningStatus)
    {
        try
        {
            return await next.Update(ocrScreeningStatus);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in {name}", nameof(Update));
            throw;
        }
    }
}