using AktBob.Database.Contracts;
using AktBob.Database.Entities;
using Microsoft.Extensions.Logging;

namespace AktBob.Database.Repositories;

internal class MessageRepositoryLoggingDecorator(IMessageRepository inner, ILogger<MessageRepositoryLoggingDecorator> logger) : IMessageRepository
{
    private readonly IMessageRepository _inner = inner;
    private readonly ILogger<MessageRepositoryLoggingDecorator> _logger = logger;

    public async Task<int> Add(Message message)
    {
        _logger.LogInformation("Adding {message}", message);

        var rowsAffected = await _inner.Add(message);
        if (rowsAffected == 0)
        {
            _logger.LogWarning("No rows were affected when trying to add {message}", message);
        }

        return rowsAffected;
    }

    public async Task<int> Delete(int id)
    {
        _logger.LogInformation("Marking message {id} as deleted", id);

        var rowsAffected = await _inner.Delete(id);

        if (rowsAffected == 0)
        {
            _logger.LogWarning("No rows affected when trying to marked message {id} as deleted", id);
        }

        if (rowsAffected > 1)
        {
            _logger.LogCritical("{count} rows affected when marking message {id} as deleted", rowsAffected, id);
        }

        return rowsAffected;
    }

    public async Task<Message?> GetByDeskproMessageId(int deskproMessageId)
    {
        _logger.LogInformation("Getting message by DeskproMessageId {id}", deskproMessageId);

        var message = await _inner.GetByDeskproMessageId(deskproMessageId);
        if (message is null)
        {
            _logger.LogWarning("Message by DeskproMessageId {id} not found in database", deskproMessageId);
        }

        return message;
    }

    public async Task<Message?> Get(int id)
    {
        _logger.LogInformation("Getting message {id}", id);

        var message = await _inner.Get(id);
        if (message is null)
        {
            _logger.LogWarning("Message {id} not found in database", id);
        }

        return message;
    }

    public async Task<int> Update(Message message)
    {
        _logger.LogInformation("Updating {message}", message);

        var rowsAffected = await _inner.Update(message);
        if (rowsAffected == 0)
        {
            _logger.LogWarning("No rows were affected when trying to update {message}", message);
        }

        return rowsAffected;
    }
}
