using AktBob.Deskpro.Contracts.DTOs;
using AktBob.Shared;
using AktBob.Shared.Types.Deskpro;

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

    public async Task<ErrorOr<IReadOnlyCollection<AttachmentDto>>> Handle(MessageId messageId, CancellationToken cancellationToken)
    {
        var cacheKey = $"Deskpro_MessageAttachments_{messageId.TicketId}_{messageId.Id}";
        var cachedMessageAttachments = _cache.Get<IReadOnlyCollection<AttachmentDto>>(cacheKey);
        if (cachedMessageAttachments != null)
        {
            return cachedMessageAttachments.ToErrorOr();
        }

        var result = await _inner.Handle(messageId, cancellationToken);
        if (!result.IsError)
        {
            _cache.Set(cacheKey, result.Value, TimeSpan.FromDays(5));
        }

        return result;
    }
}