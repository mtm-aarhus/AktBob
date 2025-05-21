using AktBob.Deskpro.Contracts;
using AktBob.Deskpro.Contracts.DTOs;
using AktBob.Shared;

namespace AktBob.Deskpro.Handlers.GetPersonById;
internal class GetPersonByIdHandlerCaching : IGetPersonByIdHandler
{
    private readonly IGetPersonByIdHandler _inner;
    private readonly ICacheService _cache;

    public GetPersonByIdHandlerCaching(IGetPersonByIdHandler inner, ICacheService cache)
    {
        _inner = inner;
        _cache = cache;
    }

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