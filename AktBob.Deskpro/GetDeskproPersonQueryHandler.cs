using AAK.Deskpro;
using AAK.Deskpro.Models;
using Ardalis.Result;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using AktBob.Deskpro.Contracts;

namespace AktBob.Deskpro;
internal class GetDeskproPersonQueryHandler : IRequestHandler<GetDeskproPersonQuery, Result<Person>>
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

    public async Task<Result<Person>> Handle(GetDeskproPersonQuery request, CancellationToken cancellationToken)
    {
        var person = await _deskpro.GetPersonById(request.PersonId);

        if (person is null || string.IsNullOrEmpty(person.Email))
        {
            _logger.LogError("Deskpro did not return any email address for person {personId}", request.PersonId);
            return Result.Error();
        }

        _logger.LogInformation("Deskpro personId {personId} found", request.PersonId);
        return Result.Success(person);
    }
}
