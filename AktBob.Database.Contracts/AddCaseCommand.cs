using AktBob.Database.Contracts.Dtos;
using Ardalis.Result;
using MassTransit.Mediator;

namespace AktBob.Database.UseCases.Cases.AddCase;
public record AddCaseCommand(int TicketId, long PodioItemId, string CaseNumber, Guid? FilArkivCaseId) : Request<Result<CaseDto>>;
