using AktBob.Shared;
using AktBob.Shared.Contracts.Modules.Deskpro.DTOs;

namespace Aktbob.Modules.Deskpro.Features.GetMessageAttachments;
internal class GetMessageAttachmentsHandlerCaching(
    IGetMessageAttachmentsHandler inner,
    ICacheService cache) : IGetMessageAttachmentsHandler
{
    private readonly IGetMessageAttachmentsHandler _inner = inner;
    private readonly ICacheService _cache = cache;

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