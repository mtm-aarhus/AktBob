using AktBob.Shared.Contracts.Modules.Deskpro.DTOs;
using AktBob.Shared.Extensions;

namespace Aktbob.Modules.Deskpro.Features.GetPersonByEmail;
internal class GetPersonByEmailHandlerLogging(IGetPersonByEmailHandler inner, ILogger<GetPersonByEmailHandler> logger) : IGetPersonByEmailHandler
{
    private readonly IGetPersonByEmailHandler _inner = inner;
    private readonly ILogger<GetPersonByEmailHandler> _logger = logger;

    public async Task<ErrorOr<PersonDto>> Handle(string email, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting Deskpro person by email {email}", email);

        var result = await _inner.Handle(email, cancellationToken);

        result.Switch(
            _ => _logger.LogInformation("Deskpro person by email {email} retrieved", email),
            errors => _logger.LogDebug("{name}: {errors}", nameof(GetPersonByEmailHandler), errors.ToCommaDelimitedString()));

        return result;
    }
}
