namespace AktBob.Deskpro.Contracts;
public interface IGetDeskproMessageAttachmentHandler
{
    Task<Result<Stream>> Handle(string downloadUrl, CancellationToken cancellationToken);
}