namespace AktBob.Deskpro.Contracts;
internal interface IGetMessageAttachmentsHandler
{
    Task<Result<IReadOnlyCollection<AttachmentDto>>> Handle(int ticketId, int messageId, CancellationToken cancellationToken);
}