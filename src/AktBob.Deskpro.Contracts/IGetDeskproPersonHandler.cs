using AktBob.Deskpro.Contracts.DTOs;

namespace AktBob.Deskpro.Contracts;
public interface IGetDeskproPersonHandler
{
    Task<Result<PersonDto>> Handle(int personId, CancellationToken cancellationToken);
}