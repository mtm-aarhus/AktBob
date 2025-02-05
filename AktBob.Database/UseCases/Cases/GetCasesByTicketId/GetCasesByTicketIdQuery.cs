using AktBob.Database.Entities;
using Ardalis.Result;
using MassTransit.Mediator;

namespace AktBob.Database.UseCases.Cases.GetCasesByTicketId;
internal record GetCasesByTicketIdQuery(int TicketId) : Request<Result<IEnumerable<Case>>>;
