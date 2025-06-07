using Aktbob.Modules.Deskpro.Contracts.DTOs;
using AktBob.Shared;
using AktBob.Shared.Types.Deskpro;

namespace Aktbob.Modules.Deskpro.Features.GetMessageAttachments;
internal class GetMessageAttachmentsHandlerCaching(
    IGetMessageAttachmentsHandler inner,
    ICacheService cache) : IGetMessageAttachmentsHandler
{
    private readonly IGetMessageAttachmentsHandler _inner = inner;
    private readonly ICacheService _cache = cache;

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