using AktBob.GetOrganized.Contracts;
using AktBob.Shared.Extensions;
using System.Collections.ObjectModel;
using Aktbob.Modules.Deskpro.Features.DownloadMessageAttachment;
using AktBob.Shared.Contracts.Modules.Deskpro.DTOs;

namespace AktBob.Workflows.Processes.AddMessageToGetOrganized;

internal record ProcessMessageAttachmentsJob(int ParentDocumentId, string CaseNumber, DateTime Timestamp, UploadDocumentCategory DocumentCategory, IEnumerable<AttachmentDto> Attachments);

internal class ProcessMessageAttachments(IServiceScopeFactory serviceScopeFactory, ILogger<ProcessMessageAttachments> logger) : IJobHandler<ProcessMessageAttachmentsJob>
{
    private readonly ILogger<ProcessMessageAttachments> _logger = logger;
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;

    public async Task Handle(ProcessMessageAttachmentsJob job, CancellationToken cancellationToken = default)
    {
        // Validate job parameters
        Guard.Against.Zero(job.ParentDocumentId);
        Guard.Against.NullOrEmpty(job.CaseNumber);

        _logger.LogInformation("Handling attachments for GetOrganized case {caseNumber} document {id}", job.CaseNumber, job.ParentDocumentId);
        
        using var scope = _serviceScopeFactory.CreateScope();
        var deskproDownloadMessageAttachmentHandler = scope.ServiceProvider.GetRequiredService<IDownloadMessageAttachmentHandler>();
        
        // var deskproModule = scope.ServiceProvider.GetRequiredServiceOrThrow<IDeskproModule>();
        var getOrganized = scope.ServiceProvider.GetRequiredServiceOrThrow<IGetOrganizedModule>();

        DateTime createdAtDanishTime = job.Timestamp.UtcToDanish();
        var childrenDocumentIds = new Collection<int>();

        foreach (var attachment in job.Attachments)
        {
            _logger.LogInformation("Adding attachment for GetOrganized case {caseNumber} document {documentId}: {filename}", job.CaseNumber, job.ParentDocumentId, attachment.FileName);
            
            using var stream = new MemoryStream();

            // Get the individual attachments from Deskpro
            var getAttachmentStreamResult = await deskproDownloadMessageAttachmentHandler.Handle(attachment.DownloadUrl, cancellationToken);
            if (getAttachmentStreamResult.IsError) throw new BusinessException($"Unable to download message attachment '{attachment.FileName}' from Deskpro message {attachment.MessageId}");

            getAttachmentStreamResult.Value.CopyTo(stream);
            var attachmentBytes = stream.ToArray();

            // Upload the attachment to GO
            var filenameNoExtension = Path.GetFileNameWithoutExtension(attachment.FileName);
            var fileExtension = Path.GetExtension(attachment.FileName);
            var filename = $"{filenameNoExtension} ({job.Timestamp:dd-MM-yyyy HH-mm-ss}){fileExtension}";

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
            
            _logger.LogInformation("Attachmenta added for GetOrganized case {caseNumber} document {documentId}: {filename}", job.CaseNumber, job.ParentDocumentId, attachment.FileName);
        }

        if (childrenDocumentIds.Count > 0)
        {
            // Set attachments as children
            await getOrganized.RelateDocuments(job.ParentDocumentId, childrenDocumentIds.ToArray(), cancellationToken);
            _logger.LogInformation("Setting attachments as children for GetOrganized case {caseNumber} document {documentId}", job.CaseNumber, job.ParentDocumentId);
            
        }

        // Finalize the parent document
        // The parent document must not be finalized before the attachments has been set as children
        getOrganized.FinalizeDocument(job.ParentDocumentId, false);
    }
}