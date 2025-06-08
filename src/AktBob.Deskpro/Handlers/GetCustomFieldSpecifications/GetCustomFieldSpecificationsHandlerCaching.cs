using AktBob.Deskpro.Contracts.DTOs;
using AktBob.Shared;

namespace AktBob.Deskpro.Handlers.GetCustomFieldSpecifications;
internal class GetCustomFieldSpecificationsHandlerCaching : IGetCustomFieldSpecificationsHandler
{
    private readonly IGetCustomFieldSpecificationsHandler _inner;
    private readonly ICacheService _cache;

    public GetCustomFieldSpecificationsHandlerCaching(IGetCustomFieldSpecificationsHandler inner, ICacheService cache)
    {
        _inner = inner;
        _cache = cache;
    }

    public async Task<ErrorOr<IReadOnlyCollection<CustomFieldSpecificationDto>>> Handle(CancellationToken cancellationToken)
    {
        var cacheKey = "Deskpro_CustomFieldSpecifications";
        var cachedCustomSpecifications = _cache.Get<IReadOnlyCollection<CustomFieldSpecificationDto>?>(cacheKey);
        if (cachedCustomSpecifications != null && cachedCustomSpecifications.Any())
        {
            return cachedCustomSpecifications.ToErrorOr();
        }

        var result = await _inner.Handle(cancellationToken);
        if (!result.IsError)
        {
            _cache.Set(cacheKey, result.Value, TimeSpan.FromDays(5));
        }

        return result;
    }
}