using AAK.Deskpro;
using AAK.Deskpro.Models;
using Ardalis.Result;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AktBob.CheckOCRScreeningStatus.UseCases.GetDeskproPerson;
internal class GetDeskproPersonQueryHandler : IRequestHandler<GetDeskproPersonQuery, Result<Person>>
{
    private readonly IData _data;
    private readonly IDeskproClient _deskpro;
    private readonly IConfiguration _configuration;
    private readonly ILogger<GetDeskproPersonQueryHandler> _logger;
    private readonly IMediator _mediator;

    public GetDeskproPersonQueryHandler(IData data, IDeskproClient deskpro, IConfiguration configuration, ILogger<GetDeskproPersonQueryHandler> logger, IMediator mediator)
    {
        _data = data;
        _deskpro = deskpro;
        _configuration = configuration;
        _logger = logger;
        _mediator = mediator;
    }

    public async Task<Result<Person>> Handle(GetDeskproPersonQuery request, CancellationToken cancellationToken)
    {
        var person = await _deskpro.GetPersonById((int)request.PersonId);

        if (person is null || string.IsNullOrEmpty(person.Email))
        {
            _logger.LogError("Deskpro did not return any email address for person {personId}", request.PersonId);
            return Result.Error();
        }

        _logger.LogInformation("Deskpro personId {personId} found", request.PersonId);
        return Result.Success(person);
    }
}
