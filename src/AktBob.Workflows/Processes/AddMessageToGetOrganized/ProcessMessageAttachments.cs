using AktBob.Deskpro.Contracts;
using AktBob.Deskpro.Contracts.DTOs;
using AktBob.GetOrganized.Contracts;
using AktBob.Shared.Exceptions;
using AktBob.Shared.Extensions;
using System.Collections.ObjectModel;

namespace AktBob.Workflows.Processes.AddMessageToGetOrganized;

internal record ProcessMessageAttachmentsJob(int ParentDocumentId, string CaseNumber, DateTime Timestamp, UploadDocumentCategory DocumentCategory, IEnumerable<AttachmentDto> Attachments);
internal class ProcessMessageAttachments(IServiceScopeFactory serviceScopeFactory, ILogger<ProcessMessageAttachments> logger) : IJobHandler<ProcessMessageAttachmentsJob>
{
    private readonly IServiceScopeFactory serviceScopeFactory = serviceScopeFactory;
    private readonly ILogger<ProcessMessageAttachments> _logger = logger;

    public async Task Handle(ProcessMessageAttachmentsJob job, CancellationToken cancellationToken = default)
    {
        // Validate job parameters
        Guard.Against.Zero(job.ParentDocumentId);
        Guard.Against.NullOrEmpty(job.CaseNumber);

        using var scope = serviceScopeFactory.CreateScope();
        var deskproModule = scope.ServiceProvider.GetRequiredService<IDeskproModule>();
        var getOrganized = scope.ServiceProvider.GetRequiredService<IGetOrganizedModule>();

        DateTime createdAtDanishTime = job.Timestamp.UtcToDanish();
        var childrenDocumentIds = new Collection<int>();

        foreach (var attachment in job.Attachments)
        {
            using var stream = new MemoryStream();

            // Get the individual attachments from Deskpro
            var getAttachmentStreamResult = await deskproModule.GetMessageAttachment(attachment.DownloadUrl, cancellationToken);
            if (!getAttachmentStreamResult.IsSuccess) throw new BusinessException($"Unable to download message attachment '{attachment.FileName}' from Deskpro message {attachment.MessageId}, ticketId {attachment.TicketId}");

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
            if (!uploadedDocumentIdResult.IsSuccess) throw new BusinessException($"Unable to upload message attachment to GetOrganized (Filename: '{attachment.FileName}' Download URL: {attachment.DownloadUrl})");

            childrenDocumentIds.Add(uploadedDocumentIdResult.Value);

            // Finalize the attachment
            var finalizeDocumentCommand = new FinalizeDocumentCommand(uploadedDocumentIdResult.Value);
            getOrganized.FinalizeDocument(finalizeDocumentCommand);
        }

        if (childrenDocumentIds.Count > 0)
        {
            // Set attachments as children
            var relatedDocumentsCommand = new RelateDocumentsCommand(job.ParentDocumentId, childrenDocumentIds.ToArray());
            await getOrganized.RelateDocuments(relatedDocumentsCommand, cancellationToken);
        }

        // Finalize the parent document
        // The parent document must not be finalized before the attachments has been set as children
        var finalizeParentDocumentCommand = new FinalizeDocumentCommand(job.ParentDocumentId);
        getOrganized.FinalizeDocument(finalizeParentDocumentCommand);
    }
}