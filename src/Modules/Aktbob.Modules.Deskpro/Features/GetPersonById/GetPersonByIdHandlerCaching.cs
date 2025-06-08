using AktBob.Shared;
using AktBob.Shared.Contracts.Modules.Deskpro.DTOs;

namespace Aktbob.Modules.Deskpro.Features.GetPersonById;
internal class GetPersonByIdHandlerCaching(IGetPersonByIdHandler inner, ICacheService cache) : IGetPersonByIdHandler
{
    private readonly IGetPersonByIdHandler _inner = inner;
    private readonly ICacheService _cache = cache;

    public async Task<ErrorOr<PersonDto>> Handle(int personId, CancellationToken cancellationToken)
    {
        var cacheKey = $"Deskpro_Person_{personId}";

        var cachedPerson = _cache.Get<PersonDto>(cacheKey);
        if (cachedPerson != null)
        {
            return cachedPerson.ToErrorOr();
        }

        var result = await _inner.Handle(personId, cancellationToken);
        if (!result.IsError)
        {
            _cache.Set(cacheKey, result.Value, TimeSpan.FromDays(20));
        }

        return result;
    }
}