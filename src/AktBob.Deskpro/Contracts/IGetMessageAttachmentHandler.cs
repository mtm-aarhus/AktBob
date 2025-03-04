namespace AktBob.Deskpro.Contracts;
internal interface IGetMessageAttachmentHandler
{
    Task<Result<Stream>> Handle(string downloadUrl, CancellationToken cancellationToken);
}