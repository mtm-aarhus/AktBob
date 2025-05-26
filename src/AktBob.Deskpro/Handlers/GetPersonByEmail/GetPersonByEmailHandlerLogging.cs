using AktBob.Deskpro.Contracts.DTOs;
using AktBob.Deskpro.Handlers.GetPerson;
using AktBob.Shared.Extensions;

namespace AktBob.Deskpro.Handlers.GetPersonByEmail;
internal class GetPersonByEmailHandlerLogging : IGetPersonByEmailHandler
{
    private readonly IGetPersonByEmailHandler _inner;
    private readonly ILogger<GetPersonByEmailHandler> _logger;

    public GetPersonByEmailHandlerLogging(IGetPersonByEmailHandler inner, ILogger<GetPersonByEmailHandler> logger)
    {
        _inner = inner;
        _logger = logger;
    }

    public async Task<ErrorOr<PersonDto>> Handle(string email, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting Deskpro person by email {email}", email);

        var result = await _inner.Handle(email, cancellationToken);

        result.Switch(
            _ => _logger.LogInformation("Deskpro person by email {email} retrieved", email),
            errors => _logger.LogDebug("{name}: {errors}", nameof(GetPerson), errors.ToCommaDelimitedString()));

        return result;
    }
}
