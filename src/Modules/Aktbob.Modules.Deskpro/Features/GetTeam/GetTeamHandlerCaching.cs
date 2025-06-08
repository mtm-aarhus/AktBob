using AktBob.Shared;
using AktBob.Shared.Contracts.Modules.Deskpro.DTOs;

namespace Aktbob.Modules.Deskpro.Features.GetTeam;
internal class GetTeamHandlerCaching(IGetTeamHandler inner, ICacheService cache) : IGetTeamHandler
{
    private readonly IGetTeamHandler _inner = inner;
    private readonly ICacheService _cache = cache;

    public async Task<ErrorOr<TeamDto>> Handle(int id, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"Deskpro_Team_{id}";

        var cachedTeam = _cache.Get<TeamDto>(cacheKey);
        if (cachedTeam != null)
        {
            return cachedTeam;
        }

        var result = await _inner.Handle(id, cancellationToken);
        if (!result.IsError)
        {
            _cache.Set(cacheKey, result.Value, TimeSpan.FromDays(20));
        }

        return result;
    }
}
