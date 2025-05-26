namespace AktBob.Shared.Jobs;
public record UpdateDatabaseTicketJob(int Id, string? CaseNumber, string? CaseUrl, string? SharepointFolderName);