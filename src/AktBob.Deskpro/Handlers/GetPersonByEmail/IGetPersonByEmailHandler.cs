using AktBob.Deskpro.Contracts.DTOs;

namespace AktBob.Deskpro.Handlers.GetPerson;
internal interface IGetPersonByEmailHandler
{
    Task<ErrorOr<PersonDto>> Handle(string email, CancellationToken cancellationToken);
}