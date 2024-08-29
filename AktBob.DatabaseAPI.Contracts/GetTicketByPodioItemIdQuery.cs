using Ardalis.Result;
using MediatR;

namespace AktBob.DatabaseAPI.Contracts;

public record GetTicketByPodioItemIdQuery(long PodioItemId) : IRequest<Result<IEnumerable<TicketDto>>>;