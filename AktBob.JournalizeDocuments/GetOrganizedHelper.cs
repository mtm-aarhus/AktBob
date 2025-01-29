using AAK.GetOrganized.UploadDocument;
using AAK.GetOrganized;
using Ardalis.Result;
using Microsoft.Extensions.Logging;
using AktBob.Deskpro.Contracts.DTOs;
using AktBob.Deskpro.Contracts;
using MediatR;

namespace AktBob.JournalizeDocuments;
internal class GetOrganizedHelper(ILogger<GetOrganizedHelper> logger, IGetOrganizedClient getOrganizedClient, IMediator mediator)
{
    private readonly ILogger<GetOrganizedHelper> _logger = logger;
    private readonly IGetOrganizedClient _getOrganizedClient = getOrganizedClient;
    private readonly IMediator _mediator = mediator;

    public async Task<Result<int>> UploadDocumentToGO(byte[] bytes, string caseNumber, string listName, string fileName, UploadDocumentMetadata metadata, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Uploading document to GetOrganized (CaseNumber: {caseNumber}, FileName: '{filename}', file size (bytes): {filesize}) ...", caseNumber, fileName, bytes.Length);

        var result = await _getOrganizedClient.UploadDocument(
                            bytes,
                            caseNumber,
                            listName,
                            "Dokumenter",
                            fileName,
                            metadata,
                            cancellationToken);

        if (result is not null)
        {
            return result.DocumentId;
        }

        _logger.LogError("Error uploading document to GetOrganized (CaseNumber: {caseNumber}, FileName: '{filename}', file size (bytes): {filesize})", caseNumber, fileName, bytes.Length);
        return Result.Error();
    }



    public async Task ProcessAttachments(IEnumerable<AttachmentDto> attachments, string caseNumber, UploadDocumentMetadata metadata, int? parentDocumentId, CancellationToken cancellationToken = default)
    {
        if (!attachments.Any())
        {
            return;
        }

        var childrenDocumentIds = new List<int>();

        foreach (var attachment in attachments)
        {
            // Get the individual attachments as a stream
            using (var stream = new MemoryStream())
            {
                var getAttachmentStreamQuery = new GetDeskproMessageAttachmentQuery(attachment.DownloadUrl);
                var getAttachmentStreamResult = await _mediator.Send(getAttachmentStreamQuery, cancellationToken);

                if (!getAttachmentStreamResult.IsSuccess)
                {
                    _logger.LogError("Error downloading attachment '{filename}' from Deskpro message #{messageId}, ticketId {ticketId}", attachment.FileName, attachment.MessageId, attachment.TicketId);
                    continue;
                }

                getAttachmentStreamResult.Value.CopyTo(stream);
                var attachmentBytes = stream.ToArray();

                // Upload the attachment to GO
                var uploadDocumentResult = await UploadDocumentToGO(attachmentBytes, caseNumber, "Dokumenter", string.Empty, attachment.FileName, metadata, cancellationToken);
                if (!uploadDocumentResult.IsSuccess)
                {
                    continue;
                }

                // Finalize the attachment
                await _getOrganizedClient.FinalizeDocument(uploadDocumentResult.Value, false, cancellationToken);
                childrenDocumentIds.Add(uploadDocumentResult.Value);
            }
        }

        // Set attachments as children
        if (childrenDocumentIds.Count > 0)
        {
            await _getOrganizedClient.RelateDocuments((int)parentDocumentId!, childrenDocumentIds.ToArray());
        }
    }


    public async Task FinalizeDocument(int documentId, CancellationToken cancellationToken = default)
    {
        await _getOrganizedClient.FinalizeDocument(documentId, false, cancellationToken);
    }

}
