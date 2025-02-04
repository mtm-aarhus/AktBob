using AktBob.DatabaseAPI.Contracts.DTOs;
using Ardalis.Result;
using MassTransit.Mediator;

namespace AktBob.DatabaseAPI.Contracts.Commands;
public record PostCaseCommand(int TicketId, long? PodioItemId, string CaseNumber, Guid? FilArkivCaseId) : Request<Result<CaseDto>>;