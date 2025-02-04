using AktBob.Deskpro.Contracts.DTOs;
using Ardalis.Result;
using MassTransit.Mediator;

namespace AktBob.Deskpro.Contracts;
public record GetDeskproTicketByIdQuery(int Id) : Request<Result<TicketDto>>;