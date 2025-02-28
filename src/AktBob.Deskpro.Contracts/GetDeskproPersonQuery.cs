using AktBob.Deskpro.Contracts.DTOs;

namespace AktBob.Deskpro.Contracts;
public record GetDeskproPersonQuery(int PersonId) : IRequest<Result<PersonDto>>;