using AktBob.Shared;
using AktBob.Shared.Types.Deskpro;

namespace AktBob.Deskpro.Handlers.GetMessage;
internal class GetMessageHandlerCaching : IGetMessageHandler
{
    private readonly IGetMessageHandler _inner;
    private readonly ICacheService _cache;

    public GetMessageHandlerCaching(IGetMessageHandler inner, ICacheService cache)
    {
        _inner = inner;
        _cache = cache;
    }

    public async Task<ErrorOr<MessageDto>> Handle(MessageId messageId, CancellationToken cancellationToken)
    {
        var cacheKey = $"Deskpro_Message_{messageId.TicketId}_{messageId.Id}";
        var cachedMessage = _cache.Get<MessageDto>(cacheKey);

        if (cachedMessage != null)
        {
            return cachedMessage;
        }

        var result = await _inner.Handle(messageId, cancellationToken);
        if (!result.IsError)
        {
            _cache.Set(cacheKey, result.Value, TimeSpan.FromDays(5));
        }

        return result;
    }
}
