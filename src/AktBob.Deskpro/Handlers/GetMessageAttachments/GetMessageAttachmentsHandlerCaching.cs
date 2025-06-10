using AktBob.Deskpro.Contracts.DTOs;
using AktBob.Shared;

namespace AktBob.Deskpro.Handlers.GetMessageAttachments;
internal class GetMessageAttachmentsHandlerCaching : IGetMessageAttachmentsHandler
{
    private readonly IGetMessageAttachmentsHandler _inner;
    private readonly ICacheService _cache;

    public GetMessageAttachmentsHandlerCaching(IGetMessageAttachmentsHandler inner, ICacheService cache)
    {
        _inner = inner;
        _cache = cache;
    }

    public async Task<ErrorOr<IReadOnlyCollection<AttachmentDto>>> Handle(int ticketId, int messageId, CancellationToken cancellationToken)
    {
        var cacheKey = $"Deskpro_MessageAttachments_{ticketId}_{messageId}";
        var cachedMessageAttachments = _cache.Get<IReadOnlyCollection<AttachmentDto>>(cacheKey);
        if (cachedMessageAttachments != null)
        {
            return cachedMessageAttachments.ToErrorOr();
        }

        var result = await _inner.Handle(ticketId, messageId, cancellationToken);
        if (!result.IsError)
        {
            _cache.Set(cacheKey, result.Value, TimeSpan.FromDays(5));
        }

        return result;
    }
}