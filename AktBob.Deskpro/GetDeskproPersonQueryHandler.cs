using AAK.Deskpro;
using Ardalis.Result;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using AktBob.Deskpro.Contracts;
using AktBob.Deskpro.Contracts.DTOs;

namespace AktBob.Deskpro;
internal class GetDeskproPersonQueryHandler : IRequestHandler<GetDeskproPersonQuery, Result<PersonDto>>
{
    private readonly IDeskproClient _deskpro;
    private readonly IConfiguration _configuration;
    private readonly ILogger<GetDeskproPersonQueryHandler> _logger;
    private readonly IMediator _mediator;

    public GetDeskproPersonQueryHandler(IDeskproClient deskpro, IConfiguration configuration, ILogger<GetDeskproPersonQueryHandler> logger, IMediator mediator)
    {
        _deskpro = deskpro;
        _configuration = configuration;
        _logger = logger;
        _mediator = mediator;
    }

    public async Task<Result<PersonDto>> Handle(GetDeskproPersonQuery request, CancellationToken cancellationToken)
    {
        var person = await _deskpro.GetPersonById(request.PersonId);

        if (person is null)
        {
            return Result.NotFound();
        }

        var dto = new PersonDto
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

        return Result.Success(dto);
    }
}
