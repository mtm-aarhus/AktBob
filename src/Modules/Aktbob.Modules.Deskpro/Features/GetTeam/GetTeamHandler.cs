using AAK.Deskpro;
using Aktbob.Modules.Deskpro.Contracts.DTOs;

namespace Aktbob.Modules.Deskpro.Features.GetTeam;
internal class GetTeamHandler(IDeskproClient deskproClient) : IGetTeamHandler
{
    private readonly IDeskproClient _deskproClient = deskproClient;

    public async Task<ErrorOr<TeamDto>> Handle(int id, CancellationToken cancellationToken = default)
    {
        var team = await _deskproClient.GetTeam(id, cancellationToken);
        if (team == null)
        {
            return Error.Failure("GetTeamHandler.Failure", $"Error getting team {id} from Deskpro");
        }

        var dto = new TeamDto(
            TeamId: team.Id,
            Name: team.Name,
            AgentIds: team.Agents);

        return dto;
    }
}
