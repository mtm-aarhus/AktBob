using AktBob.Deskpro.Contracts;
using AktBob.Deskpro.Contracts.DTOs;
using AktBob.GetOrganized.Contracts;
using AktBob.Shared.Extensions;

namespace AktBob.Workflows.Processes.AddMessageToGetOrganized;

internal record ProcessMessageAttachmentsJob(int ParentDocumentId, string CaseNumber, DateTime Timestamp, UploadDocumentCategory DocumentCategory, IEnumerable<AttachmentDto> Attachments);
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

            var uploadDocumentCommand = new UploadDocumentCommand(
                attachmentBytes,
                job.CaseNumber,
                filename,
                string.Empty,
                createdAtDanishTime,
                job.DocumentCategory,
                true);

            var uploadedDocumentIdResult = await getOrganized.UploadDocument(uploadDocumentCommand, cancellationToken);

            if (!uploadedDocumentIdResult.IsSuccess)
            {
                _logger.LogError("Error upload Deskpro message attachment to GetOrganized (Filename: '{filename}' Download URL: {url})", attachment.FileName, attachment.DownloadUrl);
                continue;
            }

            childrenDocumentIds.Add(uploadedDocumentIdResult.Value);

            // Finalize the attachment
            var finalizeDocumentCommand = new FinalizeDocumentCommand(uploadedDocumentIdResult.Value);
            getOrganized.FinalizeDocument(finalizeDocumentCommand);
        }


        // Set attachments as children
        var relatedDocumentsCommand = new RelateDocumentsCommand(job.ParentDocumentId, childrenDocumentIds.ToArray());
        await getOrganized.RelateDocuments(relatedDocumentsCommand, cancellationToken);


        // Finalize the parent document
        // The parent document must not be finalized before the attachments has been set as children
        var finalizeParentDocumentCommand = new FinalizeDocumentCommand(job.ParentDocumentId);
        getOrganized.FinalizeDocument(finalizeParentDocumentCommand);
    }
}