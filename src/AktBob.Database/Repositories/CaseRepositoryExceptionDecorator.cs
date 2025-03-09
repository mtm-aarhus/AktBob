using AktBob.Database.Contracts;
using AktBob.Database.Entities;
using Microsoft.Extensions.Logging;

namespace AktBob.Database.Repositories;

internal class CaseRepositoryExceptionDecorator(ICaseRepository inner, ILogger<CaseRepositoryExceptionDecorator> logger) : ICaseRepository
{
    private readonly ICaseRepository _inner = inner;
    private readonly ILogger<CaseRepositoryExceptionDecorator> _logger = logger;

    public async Task<bool> Add(Case @case)
    {
        try
        {
            return await _inner.Add(@case);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {name}", nameof(Add));
            throw;
        }
    }

    public async Task<Case?> Get(int id)
    {
        try
        {
            return await _inner.Get(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {name}", nameof(Get));
            throw;
        }
    }

    public async Task<IEnumerable<Case>> GetAll(long? podioItemId, Guid? filArkivCaseId)
    {
        try
        {
            return await _inner.GetAll(podioItemId, filArkivCaseId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {name}", nameof(GetAll));
            throw;
        }
    }

    public async Task<Case?> GetByPodioItemId(long podioItemId)
    {
        try
        {
            return await _inner.GetByPodioItemId(podioItemId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {name}", nameof(GetByPodioItemId));
            throw;
        }
    }

    public async Task<Case?> GetByTicketId(int ticketId)
    {
        try
        {
            return await _inner.GetByTicketId(ticketId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {name}", nameof(GetByTicketId));
            throw;
        }
    }

    public async Task<int> Update(Case @case)
    {
        try
        {
            return await _inner.Update(@case);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {name}", nameof(Update));
            throw;
        }
    }
}
