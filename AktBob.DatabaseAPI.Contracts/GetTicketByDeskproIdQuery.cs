using Ardalis.Result;
using MediatR;

namespace AktBob.DatabaseAPI.Contracts;

public record GetTicketByDeskproIdQuery(int DeskproId): IRequest<Result<IEnumerable<TicketDto>>>;
