using AktBob.Deskpro.Contracts;
using AktBob.Deskpro.Contracts.DTOs;

namespace AktBob.Deskpro.Handlers.GetPersonById;
internal class GetPersonByIdHandlerLogging : IGetPersonByIdHandler
{
    private readonly IGetPersonByIdHandler _inner;
    private readonly ILogger<GetPersonByIdHandler> _logger;

    public GetPersonByIdHandlerLogging(IGetPersonByIdHandler inner, ILogger<GetPersonByIdHandler> logger)
    {
        _inner = inner;
        _logger = logger;
    }

    public async Task<ErrorOr<PersonDto>> Handle(int personId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting Deskpro person {id}", personId);

        var result = await _inner.Handle(personId, cancellationToken);

        result.Switch(
            _ => _logger.LogInformation("Deskpro person {id} retrieved", personId),
            errors => _logger.LogWarning("{name}: {errors}", nameof(GetPersonByIdHandler), result.Errors));

        return result;
    }
}
