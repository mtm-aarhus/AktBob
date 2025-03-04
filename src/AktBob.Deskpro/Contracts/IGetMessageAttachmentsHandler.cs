namespace AktBob.Deskpro.Contracts;
internal interface IGetMessageAttachmentsHandler
{
    Task<Result<IEnumerable<AttachmentDto>>> Handle(int ticketId, int messageId, CancellationToken cancellationToken);
}