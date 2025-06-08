using AktBob.Deskpro.Contracts.DTOs;

namespace AktBob.Deskpro.Handlers.GetTeam;
internal interface IGetTeamHandler
{
    Task<ErrorOr<TeamDto>> Handle(int id, CancellationToken cancellationToken = default);
}
