using AktBob.Database.Entities;
using Ardalis.Result;
using MediatR;

namespace AktBob.Database.UseCases.Cases.GetCasesByTicketId;
internal record GetCasesByTicketIdQuery(int TicketId) : IRequest<Result<IEnumerable<Case>>>;
