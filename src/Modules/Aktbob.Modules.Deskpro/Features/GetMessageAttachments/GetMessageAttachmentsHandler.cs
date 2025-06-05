using AAK.Deskpro;
using AktBob.Shared.Types.Deskpro;

namespace Aktbob.Modules.Deskpro.Features.GetMessageAttachments;
public class GetMessageAttachmentsHandler(IDeskproClient deskpro) : IGetMessageAttachmentsHandler
{
    private readonly IDeskproClient _deskpro = deskpro;

    public async Task<ErrorOr<IReadOnlyCollection<AttachmentDto>>> Handle(MessageId messageId, CancellationToken cancellationToken)
    {
        var attachments = new List<AttachmentDto>();
        var pageNumber = 1;
        const int attachmentsPerPage = 10;
        var totalPageCount = 1;

        // Deskpro is paginating the message attachments result
        // -> loop through all pages and add all the attachment objects to the list
        do
        {
            var pageAttachments = await _deskpro.GetMessageAttachments(messageId.TicketId, messageId.Id, pageNumber, attachmentsPerPage, cancellationToken);
            var dtos = pageAttachments.Attachments.Select(x => new AttachmentDto
            {
                IsAgentNote = x.IsAgentNote,
                BlobId = x.BlobId,
                ContentType = x.ContentType,
                DownloadUrl = x.DownloadUrl,
                FileName = x.FileName,
                Id = x.Id,
                MessageId = MessageId.Create(x.TicketId, x.MessageId),
                PersonId = x.PersonId
            });

            attachments.AddRange(dtos);

            totalPageCount = pageAttachments.Pagination.TotalPages;
            pageNumber++;
        }
        while (pageNumber <= totalPageCount);

        return attachments;
    }
}
