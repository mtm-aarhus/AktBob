using AAK.Deskpro;
using Ardalis.Result;
using AktBob.Deskpro.Contracts;
using AktBob.Deskpro.Contracts.DTOs;
using Microsoft.Extensions.Caching.Memory;
using MassTransit.Mediator;

namespace AktBob.Deskpro.UseCases;
public class GetDeskproPersonQueryHandler(IDeskproClient deskpro, IMemoryCache cache) : MediatorRequestHandler<GetDeskproPersonQuery, Result<PersonDto>>
{
    private readonly IDeskproClient _deskpro = deskpro;
    private readonly IMemoryCache _cache = cache;
    private const string CACHE_KEY = "DeskproPerson";

    protected override async Task<Result<PersonDto>> Handle(GetDeskproPersonQuery request, CancellationToken cancellationToken)
    {
        if (_cache.TryGetValue(CACHE_KEY + request.PersonId.ToString(), out PersonDto? dto))
        {
            if (dto != null)
            {
                return Result.Success(dto);
            }
        }

        var person = await _deskpro.GetPersonById(request.PersonId);

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

        _cache.Set(CACHE_KEY + request.PersonId, dto, TimeSpan.FromHours(24));

        return Result.Success(dto);
    }
}
