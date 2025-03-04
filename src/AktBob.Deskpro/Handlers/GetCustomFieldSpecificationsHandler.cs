using AAK.Deskpro;
using Microsoft.Extensions.Caching.Memory;

namespace AktBob.Deskpro.Handlers;
internal class GetCustomFieldSpecificationsHandler(
    IDeskproClient deskproClient,
    IMemoryCache cache,
    ILogger<GetCustomFieldSpecificationsHandler> logger) : IGetCustomFieldSpecificationsHandler
{
    private readonly IDeskproClient _deskproClient = deskproClient;
    private readonly IMemoryCache _cache = cache;
    private readonly ILogger<GetCustomFieldSpecificationsHandler> _logger = logger;
    private const string CACHE_KEY = "DeskproCustomFieldSpecifications";

    public async Task<Result<IEnumerable<CustomFieldSpecificationDto>>> Handle(CancellationToken cancellationToken)
    {
        if (_cache.TryGetValue(CACHE_KEY, out IEnumerable<CustomFieldSpecificationDto>? cachedSpecifications))
        {
            if (cachedSpecifications != null && cachedSpecifications.Any())
            {
                return Result.Success(cachedSpecifications);
            }
        }

        try
        {
            var dto = await _deskproClient.GetCustomFieldSpecifications(cancellationToken);
            var specifications = dto.Select(x => new CustomFieldSpecificationDto(x.Id, x.Title));

            _cache.Set(CACHE_KEY, specifications, TimeSpan.FromHours(24));

            return Result.Success(specifications);
        }
        catch (Exception ex)
        {
            _logger.LogError("Error getting custom field specification from Deskpro. Error: {error}", ex.Message);
            return Result.Error();
        }
    }
}