using AktBob.Shared.Contracts.Modules.Deskpro.DTOs;

namespace Aktbob.Modules.Deskpro.Features.GetTeam;
internal interface IGetTeamHandler
{
    Task<ErrorOr<TeamDto>> Handle(int id, CancellationToken cancellationToken = default);
}
