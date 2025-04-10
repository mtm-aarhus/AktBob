﻿using AktBob.Database.Contracts;
using AktBob.Database.Entities;
using AktBob.Database.Repositories;
using Microsoft.Extensions.Logging;

namespace AktBob.Database.Decorators;

internal class MessageRepositoryLoggingDecorator(IMessageRepository inner, ILogger<MessageRepository> logger) : IMessageRepository
{
    private readonly IMessageRepository _inner = inner;
    private readonly ILogger<MessageRepository> _logger = logger;

    public async Task<bool> Add(Message message)
    {
        _logger.LogInformation("Adding {message}", message);

        var success = await _inner.Add(message);

        if (!success)
        {
            _logger.LogDebug("{name}: No rows were affected when trying to add {message}", nameof(Add), message);
        }

        return success;
    }

    public async Task<bool> Delete(int id)
    {
        _logger.LogInformation("Marking message {id} as deleted", id);

        var success = await _inner.Delete(id);

        if (!success)
        {
            _logger.LogDebug("{name}: No rows affected when trying to marked message {id} as deleted", nameof(Delete), id);
        }

        return success;
    }

    public async Task<Message?> GetByDeskproMessageId(int deskproMessageId)
    {
        _logger.LogInformation("Getting message by DeskproMessageId {id}", deskproMessageId);

        var message = await _inner.GetByDeskproMessageId(deskproMessageId);
        if (message is null)
        {
            _logger.LogDebug("{name}: Message by DeskproMessageId {id} not found in database", nameof(GetByDeskproMessageId), deskproMessageId);
        }

        return message;
    }

    public async Task<Message?> Get(int id)
    {
        _logger.LogInformation("Getting message {id}", id);

        var message = await _inner.Get(id);
        if (message is null)
        {
            _logger.LogDebug("{name}: Message {id} not found in database", nameof(Get), id);
        }

        return message;
    }

    public async Task<bool> Update(Message message)
    {
        _logger.LogInformation("Updating {message}", message);

        var success = await _inner.Update(message);
        if (!success)
        {
            _logger.LogDebug("{name}: No rows were affected when trying to update {message}", nameof(Update), message);
        }

        return success;
    }
}
