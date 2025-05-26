using AktBob.Deskpro.Contracts.DTOs;

namespace AktBob.Deskpro.Contracts;
internal interface IGetPersonByIdHandler
{
    Task<ErrorOr<PersonDto>> Handle(int personId, CancellationToken cancellationToken);
}