using AktBob.Shared.Contracts.Modules.Deskpro.DTOs;

namespace Aktbob.Modules.Deskpro.Features.GetPersonById;
internal class GetPersonByIdHandlerException(IGetPersonByIdHandler inner, ILogger<GetPersonByIdHandler> logger) : IGetPersonByIdHandler
{
    private readonly IGetPersonByIdHandler _inner = inner;
    private readonly ILogger<GetPersonByIdHandler> _logger = logger;

    public async Task<ErrorOr<PersonDto>> Handle(int personId, CancellationToken cancellationToken)
    {
        try
        {
            return await _inner.Handle(personId, cancellationToken);
        }
        catch (HttpRequestException ex)
        when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Deskpro person {id} not found", personId);
            return Error.NotFound("GetPersonByIdHandler.NotFound", $"Deskpro person {personId} not found.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {name}", nameof(GetPersonByIdHandler));
            return Error.Failure("GetPersonByIdHandler.Failure", ex.Message);
        }
    }
}
