using AktBob.Deskpro.Contracts.DTOs;

namespace AktBob.Deskpro.Contracts;
public record GetDeskproTicketByIdQuery(int Id) : IQuery<Result<TicketDto>>;