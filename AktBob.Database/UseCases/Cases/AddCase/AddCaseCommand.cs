using AktBob.Database.Entities;
using Ardalis.Result;
using MediatR;

namespace AktBob.Database.UseCases.Cases.AddCase;
internal record AddCaseCommand(int TicketId, long PodioItemId, string CaseNumber, Guid? FilArkivCaseId) : IRequest<Result<Case>>;
