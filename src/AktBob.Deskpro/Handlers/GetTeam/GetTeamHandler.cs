using AAK.Deskpro;

namespace AktBob.Deskpro.Handlers.GetTeam;
internal class GetTeamHandler : IGetTeamHandler
{
    private readonly IDeskproClient _deskproClient;

    public GetTeamHandler(IDeskproClient deskproClient)
    {
        _deskproClient = deskproClient;
    }

    public async Task<ErrorOr<TeamDto>> Handle(int id, CancellationToken cancellationToken = default)
    {
        var team = await _deskproClient.GetTeam(id, cancellationToken);
        if (team == null)
        {
            return Error.Failure("GetTeamHandler.Failure", $"Error gettint team {id} from Deskpro");
        }

        var dto = new TeamDto(
            TeamId: team.Id,
            Name: team.Name,
            AgentIds: team.Agents);

        return dto;
    }
}
