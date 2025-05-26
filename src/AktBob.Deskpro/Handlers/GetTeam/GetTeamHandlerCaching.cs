using AktBob.Shared;

namespace AktBob.Deskpro.Handlers.GetTeam;
internal class GetTeamHandlerCaching : IGetTeamHandler
{
    private readonly IGetTeamHandler _inner;
    private readonly ICacheService _cache;

    public GetTeamHandlerCaching(IGetTeamHandler inner, ICacheService cache)
    {
        _inner = inner;
        _cache = cache;
    }

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
