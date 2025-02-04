using AktBob.Deskpro.Contracts.DTOs;
using Ardalis.Result;
using MassTransit.Mediator;

namespace AktBob.Deskpro.Contracts;
public record GetDeskproTicketsByFieldSearchQuery(int[] Fields, string SearchValue) : Request<Result<IEnumerable<TicketDto>>>;