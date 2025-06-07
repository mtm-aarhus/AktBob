using Aktbob.Modules.Deskpro.Contracts.DTOs;
using AktBob.Shared;

namespace Aktbob.Modules.Deskpro.Features.GetPersonByEmail;
internal class GetPersonByEmailHandlerCaching(IGetPersonByEmailHandler inner, ICacheService cache) : IGetPersonByEmailHandler
{
    private readonly IGetPersonByEmailHandler _inner = inner;
    private readonly ICacheService _cache = cache;

    public async Task<ErrorOr<PersonDto>> Handle(string email, CancellationToken cancellationToken)
    {
        var cacheKey = $"Deskpro_Person_{email}";

        var cachedPerson = _cache.Get<PersonDto>(cacheKey);
        if (cachedPerson != null)
        {
            return cachedPerson.ToErrorOr();
        }

        var result = await _inner.Handle(email, cancellationToken);
        if (!result.IsError)
        {
            _cache.Set(cacheKey, result.Value, TimeSpan.FromDays(20));
        }

        return result;
    }
}