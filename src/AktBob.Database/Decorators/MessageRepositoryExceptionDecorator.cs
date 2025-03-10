using AktBob.Database.Contracts;
using AktBob.Database.Entities;
using AktBob.Database.Repositories;
using Microsoft.Extensions.Logging;

namespace AktBob.Database.Decorators;

internal class MessageRepositoryExceptionDecorator(IMessageRepository inner, ILogger<MessageRepository> logger) : IMessageRepository
{
    private readonly IMessageRepository _inner = inner;
    private readonly ILogger<v> _logger = logger;

    public async Task<bool> Add(Message message)
    {
        try
        {
            return await _inner.Add(message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {name}", nameof(Add));
            throw;
        }
    }

    public async Task<int> Delete(int id)
    {
        try
        {
            return await _inner.Delete(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {name}", nameof(Delete));
            throw;
        }
    }

    public async Task<Message?> Get(int id)
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

    public async Task<Message?> GetByDeskproMessageId(int deskproMessageId)
    {
        try
        {
            return await _inner.GetByDeskproMessageId(deskproMessageId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {name}", nameof(GetByDeskproMessageId));
            throw;
        }
    }

    public async Task<int> Update(Message message)
    {
        try
        {
            return await _inner.Update(message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {name}", nameof(Update));
            throw;
        }
    }
}
