using AktBob.Shared.Contracts.Modules.Deskpro.DTOs;

namespace Aktbob.Modules.Deskpro.Features.GetPersonByEmail;
public interface IGetPersonByEmailHandler
{
    Task<ErrorOr<PersonDto>> Handle(string email, CancellationToken cancellationToken);
}