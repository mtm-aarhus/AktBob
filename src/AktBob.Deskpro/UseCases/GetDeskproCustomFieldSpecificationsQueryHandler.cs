using AAK.Deskpro;
using AktBob.Deskpro.Contracts;
using AktBob.Deskpro.Contracts.DTOs;
using Ardalis.Result;
using MassTransit.Mediator;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace AktBob.Deskpro.UseCases;
public class GetDeskproCustomFieldSpecificationsQueryHandler(
    IDeskproClient deskproClient,
    IMemoryCache cache,
    ILogger<GetDeskproCustomFieldSpecificationsQueryHandler> logger) : MediatorRequestHandler<GetDeskproCustomFieldSpecificationsQuery, Result<IEnumerable<CustomFieldSpecificationDto>>>
{
    private readonly IDeskproClient _deskproClient = deskproClient;
    private readonly IMemoryCache _cache = cache;
    private readonly ILogger<GetDeskproCustomFieldSpecificationsQueryHandler> _logger = logger;
    private const string CACHE_KEY = "DeskproCustomFieldSpecifications";

    protected override async Task<Result<IEnumerable<CustomFieldSpecificationDto>>> Handle(GetDeskproCustomFieldSpecificationsQuery query, CancellationToken cancellationToken)
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