using AktBob.Deskpro.Contracts.DTOs;
using AktBob.Deskpro.Handlers.GetPerson;
using AktBob.Shared;

namespace AktBob.Deskpro.Handlers.GetPersonByEmail;
internal class GetPersonByEmailHandlerCaching : IGetPersonByEmailHandler
{
    private readonly IGetPersonByEmailHandler _inner;
    private readonly ICacheService _cache;

    public GetPersonByEmailHandlerCaching(IGetPersonByEmailHandler inner, ICacheService cache)
    {
        _inner = inner;
        _cache = cache;
    }

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