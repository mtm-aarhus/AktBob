using AAK.Deskpro;

namespace AktBob.Deskpro.Handlers;
internal class GetTeamHandler : IGetTeamHandler
{
    private readonly IDeskproClient _deskproClient;

    public GetTeamHandler(IDeskproClient deskproClient)
    {
        _deskproClient = deskproClient;
    }

    public async Task<Result<TeamDto>> Handle(int id, CancellationToken cancellationToken = default)
    {
        var team = await _deskproClient.GetTeam(id, cancellationToken);
        if (team == null)
        {
            return Result.NotFound($"Team {id} not found in Deskpro");
        }

        var dto = new TeamDto(
            TeamId: team.Id,
            Name: team.Name,
            AgentIds: team.Agents);

        return dto;

    }
}
