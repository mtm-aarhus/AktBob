namespace AktBob.Deskpro.Contracts;
internal interface IGetPersonHandler
{
    Task<Result<PersonDto>> GetById(int personId, CancellationToken cancellationToken);
    Task<Result<PersonDto>> GetByEmail(string email, CancellationToken cancellationToken);
}