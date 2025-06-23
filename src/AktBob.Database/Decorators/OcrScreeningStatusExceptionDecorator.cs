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

    public async Task<OcrScreeningStatus?> Get(Guid filArkivFileId)
    {
        try
        {
            return await next.Get(filArkivFileId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in {name}", nameof(Get));
            throw;
        }
    }

    public async Task RemoveByCaseId(Guid filArkivCaseId)
    {
        try
        {
            await next.RemoveByCaseId(filArkivCaseId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in {name}", nameof(Get));
            throw;
        }
    }

    public async Task<bool> AnyByCaseId(Guid filarkivCaseId)
    {
        try
        {
            return await next.AnyByCaseId(filarkivCaseId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in {name}", nameof(Get));
            throw;
        }
    }

    public async Task<bool> AllFilesAreProcessed(Guid filarkivCaseId)
    {
        try
        {
            return await next.AllFilesAreProcessed(filarkivCaseId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in {name}", nameof(Get));
            throw;
        }
    }
}