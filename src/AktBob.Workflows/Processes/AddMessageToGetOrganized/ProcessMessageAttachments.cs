using AAK.GetOrganized.UploadDocument;
using AAK.GetOrganized;
using AktBob.Deskpro.Contracts;
using AktBob.Deskpro.Contracts.DTOs;
using AktBob.GetOrganized.Contracts;
using AktBob.Shared.Extensions;

namespace AktBob.Workflows.Processes.AddMessageToGetOrganized;

internal record ProcessMessageAttachmentsJob(int ParentDocumentId, string CaseNumber, DateTime Timestamp, DocumentCategory DocumentCategory, IEnumerable<AttachmentDto> Attachments);
internal class ProcessMessageAttachments(IServiceScopeFactory serviceScopeFactory, ILogger<ProcessMessageAttachments> logger) : IJobHandler<ProcessMessageAttachmentsJob>
{
    private readonly IServiceScopeFactory serviceScopeFactory = serviceScopeFactory;
    private readonly ILogger<ProcessMessageAttachments> _logger = logger;

    public async Task Handle(ProcessMessageAttachmentsJob job, CancellationToken cancellationToken = default)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var deskproModule = scope.ServiceProvider.GetRequiredService<IDeskproModule>();
        var getOrganized = scope.ServiceProvider.GetRequiredService<IGetOrganizedModule>();

        DateTime createdAtDanishTime = job.Timestamp.UtcToDanish();
        var childrenDocumentIds = new List<int>();

        var metadata = new UploadDocumentMetadata
        {
            DocumentDate = createdAtDanishTime,
            DocumentCategory = job.DocumentCategory
        };

        try
        {
            foreach (var attachment in job.Attachments)
            {
                using var stream = new MemoryStream();
                
                // Get the individual attachments from Deskpro
                var getAttachmentStreamResult = await deskproModule.GetMessageAttachment(attachment.DownloadUrl, cancellationToken);

                if (!getAttachmentStreamResult.IsSuccess)
                {
                    _logger.LogError("Error downloading attachment '{filename}' from Deskpro message #{messageId}, ticketId {ticketId}", attachment.FileName, attachment.MessageId, attachment.TicketId);
                    continue;
                }

                getAttachmentStreamResult.Value.CopyTo(stream);
                var attachmentBytes = stream.ToArray();

                // Upload the attachment to GO
                var filenameNoExtension = Path.GetFileNameWithoutExtension(attachment.FileName);
                var fileExtension = Path.GetExtension(attachment.FileName);
                var filename = $"{filenameNoExtension} ({job.Timestamp.ToString("dd-MM-yyyy HH-mm-ss")}){fileExtension}";
                var uploadedDocumentIdResult = await getOrganized.UploadDocument(attachmentBytes, job.CaseNumber, filename, metadata, true, cancellationToken);

                if (!uploadedDocumentIdResult.IsSuccess)
                {
                    _logger.LogError("Error upload Deskpro message attachment to GetOrganized (Filename: '{filename}' Download URL: {url})", attachment.FileName, attachment.DownloadUrl);
                    continue;
                }

                childrenDocumentIds.Add(uploadedDocumentIdResult.Value);

                // Finalize the attachment
                getOrganized.FinalizeDocument(uploadedDocumentIdResult.Value);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("Something went wrong uploding message attachments to GetOrganized. Message: {message}", ex.Message);
            return;

        }

        // Set attachments as children
        await getOrganized.RelateDocuments(job.ParentDocumentId, childrenDocumentIds.ToArray(), cancellationToken: cancellationToken);


        // Finalize the parent document
        // The parent document must not be finalized before the attachments has been set as children
        getOrganized.FinalizeDocument(job.ParentDocumentId);
    }
}