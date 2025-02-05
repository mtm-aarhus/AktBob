using AktBob.Database.Entities;
using Ardalis.Result;
using MediatR;

namespace AktBob.Database.UseCases.Cases.PatchCase;
internal record PatchCaseCommand(int Id, long? PodioItemId, string? CaseNumber, Guid? FilArkivCaseId, string? SharepointFolderName) : IRequest<Result<Case>>;
