using Microsoft.Extensions.Caching.Memory;

namespace AktBob.Deskpro.Decorators;

internal class ModuleCachingDecorator : IDeskproModule
{
    private readonly IDeskproModule _inner;
    private readonly ILogger<DeskproModule> _logger;
    private readonly IMemoryCache _cache;

    public ModuleCachingDecorator(IDeskproModule inner, ILogger<DeskproModule> logger, IMemoryCache cache)
    {
        _inner = inner;
        _logger = logger;
        _cache = cache;
    }

    public async Task<Result<Stream>> DownloadMessageAttachment(string downloadUrl, CancellationToken cancellationToken) => await _inner.DownloadMessageAttachment(downloadUrl, cancellationToken);
    public async Task<Result<IEnumerable<MessageDto>>> GetMessages(int ticketId, CancellationToken cancellationToken) => await _inner.GetMessages(ticketId, cancellationToken);
    public async Task<Result<TicketDto>> GetTicket(int ticketId, CancellationToken cancellationToken) => await _inner.GetTicket(ticketId, cancellationToken);
    public async Task<Result<IEnumerable<TicketDto>>> GetTicketsByFieldSearch(int[] fields, string searchValue, CancellationToken cancellationToken) => await _inner.GetTicketsByFieldSearch(fields, searchValue, cancellationToken);
    public void InvokeWebhook(string webhookId, string payload) => _inner.InvokeWebhook(webhookId, payload);

    public async Task<Result<IReadOnlyCollection<CustomFieldSpecificationDto>>> GetCustomFieldSpecifications(CancellationToken cancellationToken)
    {
        var cacheKey = "Deskpro_CustomFieldSpecifications";
    
        if (_cache.TryGetValue(cacheKey, out IReadOnlyCollection<CustomFieldSpecificationDto>? cachedSpecifications))
        {
            if (cachedSpecifications != null && cachedSpecifications.Any())
            {
                _logger.LogDebug("Got Deskpro custom field specifications from cache");
                return Result.Success(cachedSpecifications);
            }
        }

        var result = await _inner.GetCustomFieldSpecifications(cancellationToken);
        if (result.IsSuccess)
        {
            _logger.LogDebug("Setting cached value: Deskpro custom field specifications");
            _cache.Set(cacheKey, result.Value, TimeSpan.FromDays(5));
        }

        return result;
    }

    public async Task<Result<MessageDto>> GetMessage(int ticketId, int messageId, CancellationToken cancellationToken)
    {
        var cacheKey = $"Deskpro_Message_{ticketId}_{messageId}";

        if (_cache.TryGetValue(cacheKey, out MessageDto? cachedMessage))
        {
            if (cachedMessage != null)
            {
                _logger.LogDebug("Got Deskpro message from cache (ticket {ticketId} message {messageId})", ticketId, messageId);
                return cachedMessage;
            }
        }

        var result = await _inner.GetMessage(ticketId, messageId, cancellationToken);
        if (result.IsSuccess)
        {
            _logger.LogDebug("Setting cached value: Deskpro ticket {ticketId} message {messageId}", ticketId, messageId);
            _cache.Set(cacheKey, result.Value, TimeSpan.FromDays(5));
        }

        return result;
    }

    public async Task<Result<IEnumerable<AttachmentDto>>> GetMessageAttachments(int ticketId, int messageId, CancellationToken cancellationToken)
    {
        var cacheKey = $"Deskpro_MessageAttachments_{ticketId}_{messageId}";

        if (_cache.TryGetValue(cacheKey, out IEnumerable<AttachmentDto>? cachedAttachmentDtos))
        {
            if (cachedAttachmentDtos != null)
            {
                _logger.LogDebug("Got Deskpro message attachment from cache (ticket {ticketId} message {messageId}", ticketId, messageId);
                return Result.Success(cachedAttachmentDtos);
            }
        }

        var result = await _inner.GetMessageAttachments(ticketId, messageId, cancellationToken);
        if (result.IsSuccess)
        {
            _logger.LogDebug("Setting cached value: Deskpro message attachments (ticket {ticketId} message {messageId})", ticketId, messageId);
            _cache.Set(cacheKey, result.Value, TimeSpan.FromDays(5));
        }

        return result;
    }

    public async Task<Result<PersonDto>> GetPerson(int personId, CancellationToken cancellationToken)
    {
        var cacheKey = $"Deskpro_Person_{personId}";

        if (_cache.TryGetValue(cacheKey, out PersonDto? cachedPerson))
        {
            if (cachedPerson != null)
            {
                _logger.LogDebug("Got Deskpro person from cache (person {id})", personId);
                return Result.Success(cachedPerson);
            }
        }

        var result = await _inner.GetPerson(personId, cancellationToken);
        if (result.IsSuccess)
        {
            _logger.LogDebug("Setting cached value: Deskpro person {personId}.", personId);
            _cache.Set(cacheKey, result.Value, TimeSpan.FromDays(20));
        }

        return result;
    }

    public async Task<Result<PersonDto>> GetPerson(string email, CancellationToken cancellationToken)
    {
        var cacheKey = $"Deskpro_Person_{email}";

        if (_cache.TryGetValue(cacheKey, out PersonDto? cachedPerson))
        {
            if (cachedPerson != null)
            {
                _logger.LogDebug("Got Deskpro person from cache ({email})", email);
                return Result.Success(cachedPerson);
            }
        }

        var result = await _inner.GetPerson(email, cancellationToken);
        if (result.IsSuccess)
        {
            _logger.LogDebug("Setting cached value: Deskpro person {email}.", email);
            _cache.Set(cacheKey, result.Value, TimeSpan.FromDays(20));
        }

        return result;
    }
}