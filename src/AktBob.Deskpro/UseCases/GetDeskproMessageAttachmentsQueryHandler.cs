using AAK.Deskpro;
using AktBob.Deskpro.Contracts;
using AktBob.Deskpro.Contracts.DTOs;

namespace AktBob.Deskpro.UseCases;
public class GetDeskproMessageAttachmentsQueryHandler(IDeskproClient deskpro) : IRequestHandler<GetDeskproMessageAttachmentsQuery, Result<IEnumerable<AttachmentDto>>>
{
    private readonly IDeskproClient _deskpro = deskpro;

    public async Task<Result<IEnumerable<AttachmentDto>>> Handle(GetDeskproMessageAttachmentsQuery query, CancellationToken cancellationToken)
    {
        var attachments = new List<AttachmentDto>();
        var pageNumber = 1;
        var attachmentsPerPage = 10;
        var totalPageCount = 1;

        // Deskpro is paginating the message attachments result
        // -> loop through all pages and add all the attachment objects to the list
        do
        {
            var pageAttachments = await _deskpro.GetMessageAttachments(query.TicketId, query.MessageId, pageNumber, attachmentsPerPage, cancellationToken);
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

        return attachments;
    }
}
