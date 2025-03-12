namespace AktBob.Deskpro.Contracts;
internal interface IGetPersonHandler
{
    Task<Result<PersonDto>> Handle(int personId, CancellationToken cancellationToken);
    Task<Result<PersonDto>> Handle(string email, CancellationToken cancellationToken);
}