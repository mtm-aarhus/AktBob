namespace AktBob.Database.Dtos;
internal record RequestCaseDto(int DeskproId, string? CaseNumber, string? CaseUrl, string? FolderName, string? RequestDescription);