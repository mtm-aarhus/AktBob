using AAK.Deskpro;
using Microsoft.Extensions.Caching.Memory;

namespace AktBob.Deskpro.Handlers;
internal class GetPersonHandler(IDeskproClient deskpro, IMemoryCache cache) : IGetPersonHandler
{
    private readonly IDeskproClient _deskpro = deskpro;
    private readonly IMemoryCache _cache = cache;
    private const string CACHE_KEY = "DeskproPerson";

    public async Task<Result<PersonDto>> Handle(int personId, CancellationToken cancellationToken)

    {
        if (_cache.TryGetValue(CACHE_KEY + personId.ToString(), out PersonDto? dto))
        {
            if (dto != null)
            {
                return Result.Success(dto);
            }
        }

        var person = await _deskpro.GetPersonById(personId);

        if (person is null)
        {
            return Result.NotFound();
        }

        dto = new PersonDto
        {
            Id = person.Id,
            IsAgent = person.IsAgent,
            DisplayName = person.DisplayName,
            Email = person.Email,
            FirstName = person.FirstName,
            LastName = person.LastName,
            FullName = person.FullName,
            PhoneNumbers = person.PhoneNumbers
        };

        _cache.Set(CACHE_KEY + personId, dto, TimeSpan.FromHours(24));

        return Result.Success(dto);
    }
}
