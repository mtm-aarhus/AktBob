namespace AktBob.Deskpro.Contracts;
internal interface IGetTeamHandler
{
    Task<Result<TeamDto>> Handle(int id, CancellationToken cancellationToken = default);
}
