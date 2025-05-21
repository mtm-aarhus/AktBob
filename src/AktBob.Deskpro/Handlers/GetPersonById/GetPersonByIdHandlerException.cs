using AktBob.Deskpro.Contracts;
using AktBob.Deskpro.Contracts.DTOs;

namespace AktBob.Deskpro.Handlers.GetPersonById;
internal class GetPersonByIdHandlerException : IGetPersonByIdHandler
{
    private readonly IGetPersonByIdHandler _inner;
    private readonly ILogger<GetPersonByIdHandler> _logger;

    public GetPersonByIdHandlerException(IGetPersonByIdHandler inner, ILogger<GetPersonByIdHandler> logger)
    {
        _inner = inner;
        _logger = logger;
    }
    public async Task<ErrorOr<PersonDto>> Handle(int personId, CancellationToken cancellationToken)
    {
        try
        {
            return await _inner.Handle(personId, cancellationToken);
        }
        catch (HttpRequestException ex)
        when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return Error.NotFound("GetPersonByIdHandler.NotFound", $"Deskpro person {personId} not found.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {name}", nameof(GetPersonByIdHandler));
            return Error.Failure("GetPersonByIdHandler.Failure", ex.Message);
        }
    }
}
