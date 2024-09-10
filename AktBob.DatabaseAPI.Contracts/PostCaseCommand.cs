using Ardalis.Result;
using MediatR;

namespace AktBob.DatabaseAPI.Contracts;
public record PostCaseCommand(int TicketId, long? PodioItemId, string CaseNumber, Guid? FilArkivCaseId) : IRequest<Result<CaseDto>>;