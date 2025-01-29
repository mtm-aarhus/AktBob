using AAK.GetOrganized.UploadDocument;
using AAK.GetOrganized;
using Ardalis.Result;
using Microsoft.Extensions.Logging;
using AktBob.Deskpro.Contracts.DTOs;
using AktBob.Deskpro.Contracts;
using MediatR;
using Microsoft.Extensions.Configuration;

namespace AktBob.JournalizeDocuments;
internal class GetOrganizedHelper(ILogger<GetOrganizedHelper> logger, IGetOrganizedClient getOrganizedClient, IMediator mediator, IConfiguration configuration)
{
    private readonly ILogger<GetOrganizedHelper> _logger = logger;
    private readonly IGetOrganizedClient _getOrganizedClient = getOrganizedClient;
    private readonly IMediator _mediator = mediator;
    private readonly IConfiguration _configuration = configuration;

    public async Task<Result<int>> UploadDocumentToGO(byte[] bytes, string caseNumber, string fileName, UploadDocumentMetadata metadata, bool overwrite, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Uploading document to GetOrganized (CaseNumber: {caseNumber}, FileName: '{filename}', file size (bytes): {filesize}) ...", caseNumber, fileName, bytes.Length);
        var listName = _configuration.GetValue<string>("GetOrganized:DefaultListName") ?? "Dokumenter";

        var result = await _getOrganizedClient.UploadDocument(
                            bytes,
                            caseNumber,
                            listName,
                            string.Empty,
                            fileName,
                            metadata,
                            overwrite,
                            cancellationToken);

        if (result is not null)
        {
            _logger.LogInformation("Document uploaded to GetOrganized (CaseNumber: {caseNumber}, FileName: '{filename}').", caseNumber, fileName);
            return result.DocumentId;
        }

        _logger.LogError("Error uploading document to GetOrganized (CaseNumber: {caseNumber}, FileName: '{filename}')", caseNumber, fileName);
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
                var uploadDocumentResult = await UploadDocumentToGO(attachmentBytes, caseNumber, attachment.FileName, metadata, overwrite: false, cancellationToken); // TODO: make unique filenames independent from possible file already uploaded with same file name
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
