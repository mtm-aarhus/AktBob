namespace AktBob.Shared.Jobs;
public record UpdateDatabaseCaseJob(int Id, long? PodioItemId, string? CaseNumber, Guid? FilArkivCaseId, string? SharepointFolderName);
