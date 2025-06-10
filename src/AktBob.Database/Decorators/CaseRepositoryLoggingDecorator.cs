using AktBob.Database.Contracts;
using AktBob.Database.Entities;
using AktBob.Database.Repositories;
using Microsoft.Extensions.Logging;

namespace AktBob.Database.Decorators;

internal class CaseRepositoryLoggingDecorator(ICaseRepository inner, ILogger<CaseRepository> logger) : ICaseRepository
{
    private readonly ICaseRepository _inner = inner;
    private readonly ILogger<CaseRepository> _logger = logger;

    public async Task<bool> Add(Case @case)
    {
        _logger.LogInformation("Adding to database {case}", @case);

        var success = await _inner.Add(@case);
        if (!success)
        {
            _logger.LogDebug("{name}: No rows were affected when trying to add {case}", nameof(Add), @case);
        }

        return success;
    }

    public async Task<Case?> Get(int id)
    {
        _logger.LogInformation("Getting database case {id}", id);

        var @case = await _inner.Get(id);
        if (@case is null)
        {
            _logger.LogDebug("Database case {id} not found", @case);
        }

        return @case;
    }

    public async Task<IReadOnlyCollection<Case>> GetAll(long? podioItemId, Guid? filArkivCaseId)
    {
        _logger.LogInformation("Getting all database cases by PodioItemId = {podioItemId}, FilArkivCaseId = {filArkivCaseId}", podioItemId, filArkivCaseId);

        var cases = await _inner.GetAll(podioItemId, filArkivCaseId);
        if (!cases.Any())
        {
            if (podioItemId is null && filArkivCaseId is null)
            {
                _logger.LogDebug("{name}: No database cases found", nameof(GetAll));
            }

            if (filArkivCaseId is not null)
            {
                _logger.LogDebug("{name}: No database cases found in database by FilArkivCaseId = {filArkivCaseId}", nameof(GetAll), filArkivCaseId);
            }

            if (podioItemId is not null)
            {
                _logger.LogDebug("{name}: No database cases found in database by PodioItemId = {podioItemId}", nameof(GetAll), podioItemId);
            }
        }

        return cases;
    }

    public async Task<bool> Update(Case @case)
    {
        _logger.LogInformation("Updating database {case}", @case);

        var success = await _inner.Update(@case);
        if (!success)
        {
            _logger.LogDebug("{name}: No rows were affeceted when trying to update {case}", nameof(Update), @case);
        }

        return success;
    }
}