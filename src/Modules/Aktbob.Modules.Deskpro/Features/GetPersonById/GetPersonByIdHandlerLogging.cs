using AktBob.Shared.Contracts.Modules.Deskpro.DTOs;
using AktBob.Shared.Extensions;

namespace Aktbob.Modules.Deskpro.Features.GetPersonById;
internal class GetPersonByIdHandlerLogging(IGetPersonByIdHandler inner, ILogger<GetPersonByIdHandler> logger) : IGetPersonByIdHandler
{
    private readonly IGetPersonByIdHandler _inner = inner;
    private readonly ILogger<GetPersonByIdHandler> _logger = logger;

    public async Task<ErrorOr<PersonDto>> Handle(int personId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting Deskpro person {id}", personId);

        var result = await _inner.Handle(personId, cancellationToken);

        result.Switch(
            _ => _logger.LogInformation("Deskpro person {id} retrieved", personId),
            errors => _logger.LogWarning("{name}: {errors}", nameof(GetPersonByIdHandler), errors.ToCommaDelimitedString()));

        return result;
    }
}
