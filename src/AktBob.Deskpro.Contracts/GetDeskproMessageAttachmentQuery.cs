namespace AktBob.Deskpro.Contracts;
public record GetDeskproMessageAttachmentQuery(string DownloadUrl) : IQuery<Result<Stream>>;