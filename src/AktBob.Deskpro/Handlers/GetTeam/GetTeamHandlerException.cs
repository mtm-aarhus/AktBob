using AktBob.Deskpro.Contracts.DTOs;

namespace AktBob.Deskpro.Handlers.GetTeam;
internal class GetTeamHandlerException : IGetTeamHandler
{
    private readonly IGetTeamHandler _inner;
    private readonly ILogger<GetTeamHandler> _logger;

    public GetTeamHandlerException(IGetTeamHandler inner, ILogger<GetTeamHandler> logger)
    {
        _inner = inner;
        _logger = logger;
    }

    public async Task<ErrorOr<TeamDto>> Handle(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _inner.Handle(id, cancellationToken);
        }
        catch (HttpRequestException ex)
        when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return Error.NotFound("GetTeamHandler.NotFound", $"Deskpro team {id} not found.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {name}", nameof(GetTeamHandler));
            return Error.Failure("GetTeamHandler.Failure", ex.Message);
        }
    }
}
