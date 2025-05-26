using AktBob.Deskpro.Contracts;
using AktBob.Deskpro.Contracts.DTOs;
using AktBob.GetOrganized.Contracts;
using AktBob.Shared.Extensions;
using System.Collections.ObjectModel;

namespace AktBob.Workflows.Processes.AddMessageToGetOrganized;

internal record ProcessMessageAttachmentsJob(int ParentDocumentId, string CaseNumber, DateTime Timestamp, UploadDocumentCategory DocumentCategory, IEnumerable<AttachmentDto> Attachments);

internal class ProcessMessageAttachments(IServiceScopeFactory serviceScopeFactory) : IJobHandler<ProcessMessageAttachmentsJob>
{
    public async Task Handle(ProcessMessageAttachmentsJob job, CancellationToken cancellationToken = default)
    {
        // Validate job parameters
        Guard.Against.Zero(job.ParentDocumentId);
        Guard.Against.NullOrEmpty(job.CaseNumber);

        using var scope = serviceScopeFactory.CreateScope();
        var deskproModule = scope.ServiceProvider.GetRequiredServiceOrThrow<IDeskproModule>();
        var getOrganized = scope.ServiceProvider.GetRequiredServiceOrThrow<IGetOrganizedModule>();

        DateTime createdAtDanishTime = job.Timestamp.UtcToDanish();
        var childrenDocumentIds = new Collection<int>();

        foreach (var attachment in job.Attachments)
        {
            using var stream = new MemoryStream();

            // Get the individual attachments from Deskpro
            var getAttachmentStreamResult = await deskproModule.DownloadMessageAttachment(attachment.DownloadUrl, cancellationToken);
            if (getAttachmentStreamResult.IsError) throw new BusinessException($"Unable to download message attachment '{attachment.FileName}' from Deskpro message {attachment.MessageId}");

            getAttachmentStreamResult.Value.CopyTo(stream);
            var attachmentBytes = stream.ToArray();

            // Upload the attachment to GO
            var filenameNoExtension = Path.GetFileNameWithoutExtension(attachment.FileName);
            var fileExtension = Path.GetExtension(attachment.FileName);
            var filename = $"{filenameNoExtension} ({job.Timestamp.ToString("dd-MM-yyyy HH-mm-ss")}){fileExtension}";

            var uploadedDocumentIdResult = await getOrganized.UploadDocument(
                bytes: attachmentBytes,
                caseNumber: job.CaseNumber,
                fileName: filename,
                customProperty: string.Empty,
                documentDate: createdAtDanishTime,
                category: job.DocumentCategory,
                overwriteExisting: true,
                cancellationToken: cancellationToken);
            
            if (uploadedDocumentIdResult.IsError) throw new BusinessException(uploadedDocumentIdResult.Errors.ToCommaDelimitedString());

            childrenDocumentIds.Add(uploadedDocumentIdResult.Value);

            // Finalize the attachment
            getOrganized.FinalizeDocument(uploadedDocumentIdResult.Value, false);
        }

        if (childrenDocumentIds.Count > 0)
        {
            // Set attachments as children
            await getOrganized.RelateDocuments(job.ParentDocumentId, childrenDocumentIds.ToArray(), cancellationToken);
        }

        // Finalize the parent document
        // The parent document must not be finalized before the attachments has been set as children
        getOrganized.FinalizeDocument(job.ParentDocumentId, false);
    }
}