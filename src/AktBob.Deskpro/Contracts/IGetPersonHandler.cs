namespace AktBob.Deskpro.Contracts;
internal interface IGetPersonHandler
{
    Task<Result<PersonDto>> Handle(int personId, CancellationToken cancellationToken);
}