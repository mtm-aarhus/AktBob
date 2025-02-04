using AktBob.DatabaseAPI.Contracts.DTOs;
using Ardalis.Result;
using MassTransit.Mediator;

namespace AktBob.DatabaseAPI.Contracts.Queries;

public record GetTicketByPodioItemIdQuery(long PodioItemId) : Request<Result<IEnumerable<TicketDto>>>;