using AktBob.Shared.Contracts.Modules.Deskpro.DTOs;

namespace Aktbob.Modules.Deskpro.Features.GetTeam;
public interface IGetTeamHandler
{
    Task<ErrorOr<TeamDto>> Handle(int id, CancellationToken cancellationToken = default);
}
