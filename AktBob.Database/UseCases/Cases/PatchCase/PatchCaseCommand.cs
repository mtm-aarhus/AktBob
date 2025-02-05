using AktBob.Database.Entities;
using Ardalis.Result;
using MassTransit.Mediator;

namespace AktBob.Database.UseCases.Cases.PatchCase;
internal record PatchCaseCommand(int Id, long? PodioItemId, string? CaseNumber, Guid? FilArkivCaseId, string? SharepointFolderName) : Request<Result<Case>>;
