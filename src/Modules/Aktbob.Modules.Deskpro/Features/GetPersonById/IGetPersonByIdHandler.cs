using AktBob.Shared.Contracts.Modules.Deskpro.DTOs;

namespace Aktbob.Modules.Deskpro.Features.GetPersonById;
public interface IGetPersonByIdHandler
{
    Task<ErrorOr<PersonDto>> Handle(int personId, CancellationToken cancellationToken);
}