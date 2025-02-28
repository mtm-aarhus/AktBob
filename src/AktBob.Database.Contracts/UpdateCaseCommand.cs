namespace AktBob.Database.Contracts;
public record UpdateCaseCommand(int Id, long? PodioItemId, string? CaseNumber, Guid? FilArkivCaseId, string? SharepointFolderName) : ICommand<Result<CaseDto>>;
