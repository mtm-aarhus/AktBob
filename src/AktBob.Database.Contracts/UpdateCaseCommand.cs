using AktBob.Database.Contracts.Dtos;
using Ardalis.Result;
using MassTransit.Mediator;

namespace AktBob.Database.Contracts;
public record UpdateCaseCommand(int Id, long? PodioItemId, string? CaseNumber, Guid? FilArkivCaseId, string? SharepointFolderName) : Request<Result<CaseDto>>;
