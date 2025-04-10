﻿using AktBob.Database.Contracts;
using AktBob.Database.Entities;
using AktBob.Database.Repositories;
using Microsoft.Extensions.Logging;

namespace AktBob.Database.Decorators;

internal class CaseRepositoryExceptionDecorator(ICaseRepository inner, ILogger<CaseRepository> logger) : ICaseRepository
{
    private readonly ICaseRepository _inner = inner;
    private readonly ILogger<CaseRepository> _logger = logger;

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

    public async Task<IReadOnlyCollection<Case>> GetAll(long? podioItemId, Guid? filArkivCaseId)
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

    public async Task<bool> Update(Case @case)
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
