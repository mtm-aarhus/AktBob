using AktBob.DatabaseAPI.Contracts.DTOs;
using Ardalis.Result;
using MediatR;

namespace AktBob.DatabaseAPI.Contracts.Queries;

public record GetTicketByDeskproIdQuery(int DeskproId) : IRequest<Result<IEnumerable<TicketDto>>>;
