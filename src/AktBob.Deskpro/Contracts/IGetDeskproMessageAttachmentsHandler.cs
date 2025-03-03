namespace AktBob.Deskpro.Contracts;
internal interface IGetDeskproMessageAttachmentsHandler
{
    Task<Result<IEnumerable<AttachmentDto>>> Handle(int ticketId, int messageId, CancellationToken cancellationToken);
}