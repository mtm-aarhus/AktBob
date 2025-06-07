using Aktbob.Modules.Deskpro.Contracts.DTOs;

namespace Aktbob.Modules.Deskpro.Features.GetPersonByEmail;
internal class GetPersonByEmailHandlerException(IGetPersonByEmailHandler inner, ILogger<GetPersonByEmailHandler> logger) : IGetPersonByEmailHandler
{
    private readonly IGetPersonByEmailHandler _inner = inner;
    private readonly ILogger<GetPersonByEmailHandler> _logger = logger;

    public async Task<ErrorOr<PersonDto>> Handle(string email, CancellationToken cancellationToken)
    {
        try
        {
            return await _inner.Handle(email, cancellationToken);
        }
        catch (HttpRequestException ex)
        when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return Error.NotFound("GetPersonByEmailHandler.NotFound", $"Deskpro person {email} not found.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {name}", nameof(GetPersonByEmailHandler));
            return Error.Failure("GetPersonByEmailHandler.Failure", ex.Message);
        }
    }
}
