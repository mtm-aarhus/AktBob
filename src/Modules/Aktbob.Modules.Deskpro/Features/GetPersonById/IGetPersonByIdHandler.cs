using Aktbob.Modules.Deskpro.Contracts.DTOs;

namespace Aktbob.Modules.Deskpro.Features.GetPersonById;
internal interface IGetPersonByIdHandler
{
    Task<ErrorOr<PersonDto>> Handle(int personId, CancellationToken cancellationToken);
}