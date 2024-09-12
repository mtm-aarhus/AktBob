using AktBob.DatabaseAPI.Contracts.DTOs;
using Ardalis.Result;
using MediatR;

namespace AktBob.DatabaseAPI.Contracts.Commands;
public record PostCaseCommand(int TicketId, long? PodioItemId, string CaseNumber, Guid? FilArkivCaseId) : IRequest<Result<CaseDto>>;