using AAK.Deskpro;
using Ardalis.Result;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using AktBob.Deskpro.Contracts;
using AktBob.Deskpro.Contracts.DTOs;
using Microsoft.Extensions.Caching.Memory;

namespace AktBob.Deskpro;
internal class GetDeskproPersonQueryHandler : IRequestHandler<GetDeskproPersonQuery, Result<PersonDto>>
{
    private readonly IDeskproClient _deskpro;
    private readonly IConfiguration _configuration;
    private readonly ILogger<GetDeskproPersonQueryHandler> _logger;
    private readonly IMediator _mediator;
    private readonly IMemoryCache _cache;
    private const string CACHE_KEY = "DeskproPerson";

    public GetDeskproPersonQueryHandler(IDeskproClient deskpro, IConfiguration configuration, ILogger<GetDeskproPersonQueryHandler> logger, IMediator mediator, IMemoryCache cache)
    {
        _deskpro = deskpro;
        _configuration = configuration;
        _logger = logger;
        _mediator = mediator;
        _cache = cache;
    }

    public async Task<Result<PersonDto>> Handle(GetDeskproPersonQuery request, CancellationToken cancellationToken)
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
