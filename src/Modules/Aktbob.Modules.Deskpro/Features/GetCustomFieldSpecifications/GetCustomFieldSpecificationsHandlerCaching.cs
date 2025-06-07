using AktBob.Shared;

namespace Aktbob.Modules.Deskpro.Features.GetCustomFieldSpecifications;
internal class GetCustomFieldSpecificationsHandlerCaching(
    IGetCustomFieldSpecificationsHandler inner,
    ICacheService cache) : IGetCustomFieldSpecificationsHandler
{
    private readonly IGetCustomFieldSpecificationsHandler _inner = inner;
    private readonly ICacheService _cache = cache;

    public async Task<ErrorOr<IReadOnlyCollection<CustomFieldSpecificationDto>>> Handle(CancellationToken cancellationToken)
    {
        const string cacheKey = "Deskpro_CustomFieldSpecifications";
        var cachedCustomSpecifications = _cache.Get<IReadOnlyCollection<CustomFieldSpecificationDto>?>(cacheKey);
        if (cachedCustomSpecifications != null && cachedCustomSpecifications.Count != 0)
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