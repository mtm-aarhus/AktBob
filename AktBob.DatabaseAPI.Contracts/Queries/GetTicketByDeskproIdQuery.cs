using AktBob.DatabaseAPI.Contracts.DTOs;
using Ardalis.Result;
using MassTransit.Mediator;

namespace AktBob.DatabaseAPI.Contracts.Queries;

public record GetTicketByDeskproIdQuery(int DeskproId) : Request<Result<IEnumerable<TicketDto>>>;
