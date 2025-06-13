namespace AktBob.Shared.Contracts.Database;
public record SubmissionDto(int DeskproId, string? CaseNumber, string? CaseUrl, string? FolderName, string? RequestDescription);