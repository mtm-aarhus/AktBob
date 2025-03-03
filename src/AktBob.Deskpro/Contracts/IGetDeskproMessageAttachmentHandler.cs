namespace AktBob.Deskpro.Contracts;
internal interface IGetDeskproMessageAttachmentHandler
{
    Task<Result<Stream>> Handle(string downloadUrl, CancellationToken cancellationToken);
}