using AktBob.Database.Contracts;
using AktBob.Database.Entities;
using Microsoft.Extensions.Logging;

namespace AktBob.Database.Repositories;

internal class CaseRepositoryLoggingDecorator(ICaseRepository inner, ILogger<CaseRepositoryLoggingDecorator> logger) : ICaseRepository
{
    private readonly ICaseRepository _inner = inner;
    private readonly ILogger<CaseRepositoryLoggingDecorator> _logger = logger;

    public async Task<bool> Add(Case @case)
    {
        _logger.LogInformation("Adding to database {case}", @case);

        var success = await _inner.Add(@case);
        if (!success)
        {
            _logger.LogWarning("No rows were affected when trying to add {case}", @case);
        }

        return success;
    }

    public async Task<Case?> Get(int id)
    {
        _logger.LogInformation("Getting case {id}", id);

        var @case = await _inner.Get(id);
        if (@case is null)
        {
            _logger.LogWarning("Case {id} not found in database", @case);
        }

        return @case;
    }

    public async Task<IEnumerable<Case>> GetAll(long? podioItemId, Guid? filArkivCaseId)
    {
        _logger.LogInformation("Getting all cases by PodioItemId = {podioItemId}, FilArkivCaseId = {filArkivCaseId}", podioItemId, filArkivCaseId);

        var cases = await _inner.GetAll(podioItemId, filArkivCaseId);
        if (!cases.Any())
        {
            _logger.LogWarning("No cases found in database by PodioItemId = {podioItemId}, FilArkivCaseId = {filArkivCaseId}", podioItemId, filArkivCaseId);
        }

        return cases;
    }

    public async Task<Case?> GetByPodioItemId(long podioItemId)
    {
        _logger.LogInformation("Getting case by PodioItemId {id}", podioItemId);

        var @case = await _inner.GetByPodioItemId(podioItemId);
        if (@case is null)
        {
            _logger.LogWarning("Case not found in database by PodioItemId = {id}", podioItemId);
        }

        return @case;
    }

    public async Task<Case?> GetByTicketId(int ticketId)
    {
        _logger.LogInformation("Getting case by TicketId {id}", ticketId);

        var @case = await _inner.GetByTicketId(ticketId);
        if (@case is null)
        {
            _logger.LogWarning("Case not found in database by TicketId = {id}", ticketId);
        }

        return @case;
    }

    public async Task<int> Update(Case @case)
    {
        _logger.LogInformation("Updating {case}", @case);

        var rowsAffected = await _inner.Update(@case);
        if (rowsAffected == 0)
        {
            _logger.LogWarning("No rows were affeceted when trying to update {case}", @case);
        }

        return rowsAffected;
    }
}