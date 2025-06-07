using Aktbob.Modules.Deskpro.Contracts.DTOs;
using AktBob.Shared.Extensions;

namespace Aktbob.Modules.Deskpro.Features.GetTeam;
internal class GetTeamHandlerLogging(IGetTeamHandler inner, ILogger<GetTeamHandler> logger) : IGetTeamHandler
{
    private readonly IGetTeamHandler _inner = inner;
    private readonly ILogger<GetTeamHandler> _logger = logger;

    public async Task<ErrorOr<TeamDto>> Handle(int id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting Deskpro team {id}", id);

        var result = await _inner.Handle(id, cancellationToken);

        result.Switch(
            _ => _logger.LogInformation("Deskpro team {id} retrieved", id),
            errors => _logger.LogDebug("{name}: {errors}", nameof(GetTeamHandler), errors.ToCommaDelimitedString()));

        return result;
    }
}
