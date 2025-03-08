namespace AktBob.GetOrganized.Contracts;

public record UploadDocumentCommand(
    byte[] Bytes,
    string CaseNumber,
    string FileName,
    string CustomProperty,
    DateTime DocumentDate,
    UploadDocumentCategory Category,
    bool OverwriteExisting);