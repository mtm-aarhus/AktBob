using Aktbob.Modules.Deskpro.Contracts.DTOs;

namespace Aktbob.Modules.Deskpro.Features.GetTeam;
internal class GetTeamHandlerException(IGetTeamHandler inner, ILogger<GetTeamHandler> logger) : IGetTeamHandler
{
    private readonly IGetTeamHandler _inner = inner;
    private readonly ILogger<GetTeamHandler> _logger = logger;

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
