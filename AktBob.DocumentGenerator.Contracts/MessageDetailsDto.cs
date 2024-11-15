namespace AktBob.DocumentGenerator.Contracts;
public record MessageDetailsDto(
    int MessageId,
    int MessageNumber,
    string MessageContent,
    DateTime CreatedAt,
    string PersonName,
    string PersonEmail,
    IEnumerable<string> AttachmentFileNames);
