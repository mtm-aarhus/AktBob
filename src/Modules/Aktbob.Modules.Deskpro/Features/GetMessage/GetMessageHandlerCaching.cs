using AktBob.Shared;
using AktBob.Shared.Contracts.Modules.Deskpro.DTOs;

namespace Aktbob.Modules.Deskpro.Features.GetMessage;
internal class GetMessageHandlerCaching(IGetMessageHandler inner, ICacheService cache) : IGetMessageHandler
{
    private readonly IGetMessageHandler _inner = inner;
    private readonly ICacheService _cache = cache;

    public async Task<ErrorOr<MessageDto>> Handle(int ticketId, int messageId, CancellationToken cancellationToken)
    {
        var cacheKey = $"Deskpro_Message_{ticketId}_{messageId}";
        var cachedMessage = _cache.Get<MessageDto>(cacheKey);

        if (cachedMessage != null)
        {
            return cachedMessage;
        }

        var result = await _inner.Handle(ticketId, messageId, cancellationToken);
        if (!result.IsError)
        {
            _cache.Set(cacheKey, result.Value, TimeSpan.FromDays(5));
        }

        return result;
    }
}
