using AAK.Deskpro;

namespace AktBob.Deskpro.Handlers;
public class GetMessageAttachmentsHandler(IDeskproClient deskpro) : IGetMessageAttachmentsHandler
{
    private readonly IDeskproClient _deskpro = deskpro;

    public async Task<Result<IReadOnlyCollection<AttachmentDto>>> Handle(int ticketId, int messageId, CancellationToken cancellationToken)
    {
        try
        {
            var attachments = new List<AttachmentDto>();
            var pageNumber = 1;
            var attachmentsPerPage = 10;
            var totalPageCount = 1;

            // Deskpro is paginating the message attachments result
            // -> loop through all pages and add all the attachment objects to the list
            do
            {
                var pageAttachments = await _deskpro.GetMessageAttachments(ticketId, messageId, pageNumber, attachmentsPerPage, cancellationToken);
                attachments.AddRange(pageAttachments.Attachments.Select(x => new AttachmentDto
                {
                    IsAgentNote = x.IsAgentNote,
                    BlobId = x.BlobId,
                    ContentType = x.ContentType,
                    DownloadUrl = x.DownloadUrl,
                    FileName = x.FileName,
                    Id = x.Id,
                    MessageId = x.MessageId,
                    PersonId = x.PersonId,
                    TicketId = x.TicketId
                }));

                totalPageCount = pageAttachments.Pagination.TotalPages;
                pageNumber++;
            }
            while (pageNumber <= totalPageCount);

            if (attachments is null)
            {
                return Result.Error($"Error getting message attachments list from Deskpro (ticket {ticketId} message {messageId})");
            }

            return attachments;
        }
        catch (HttpRequestException ex)
        {
            return Result.Error($"Error getting ticket {ticketId} messageId {messageId} attachments: {ex}");
        }
        catch (Exception)
        {
            throw;
        }
    }
}
