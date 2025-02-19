using AAK.GetOrganized.UploadDocument;
using AAK.GetOrganized;
using AktBob.Deskpro.Contracts;
using AktBob.Deskpro.Contracts.DTOs;
using AktBob.GetOrganized.Contracts;
using MassTransit;
using MassTransit.Mediator;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using AktBob.Shared;
using Hangfire;

namespace AktBob.JobHandlers.Handlers.AddMessageToGetOrganized;
internal class ProcessMessageAttachments(IServiceScopeFactory serviceScopeFactory, ILogger<ProcessMessageAttachments> logger)
{
    private readonly IServiceScopeFactory serviceScopeFactory = serviceScopeFactory;
    private readonly ILogger<ProcessMessageAttachments> _logger = logger;

    public async Task UploadToGetOrganized(int parentDocumentId, string caseNumber, DateTime timestamp, DocumentCategory documentCategory, IEnumerable<AttachmentDto> attachments, CancellationToken cancellationToken = default)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        DateTime createdAtDanishTime = timestamp.UtcToDanish();
        var childrenDocumentIds = new List<int>();

        var metadata = new UploadDocumentMetadata
        {
            DocumentDate = createdAtDanishTime,
            DocumentCategory = documentCategory
        };

        try
        {
            foreach (var attachment in attachments)
            {
                using var stream = new MemoryStream();
                
                // Get the individual attachments from Deskpro
                var getAttachmentStreamQuery = new GetDeskproMessageAttachmentQuery(attachment.DownloadUrl);
                var getAttachmentStreamResult = await mediator.SendRequest(getAttachmentStreamQuery, cancellationToken);

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
                var filename = $"{filenameNoExtension} ({timestamp.ToString("dd-MM-yyyy HH-mm-ss")}){fileExtension}";
                var uploadDocumentCommand = new UploadDocumentCommand(attachmentBytes, caseNumber, filename, metadata, true);
                var uploadDocumentResult = await mediator.SendRequest(uploadDocumentCommand, cancellationToken);

                if (!uploadDocumentResult.IsSuccess)
                {
                    _logger.LogError("Error upload Deskpro message attachment to GetOrganized (Filename: '{filename}' Download URL: {url})", attachment.FileName, attachment.DownloadUrl);
                    continue;
                }

                childrenDocumentIds.Add(uploadDocumentResult.Value);

                // Finalize the attachment
                BackgroundJob.Enqueue<FinalizeDocument>(x => x.Run(uploadDocumentResult.Value, CancellationToken.None));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("Something went wrong uploding message attachments to GetOrganized. Message: {message}", ex.Message);
            return;

        }

        // Set attachments as children
        var relateDocumentCommand = new RelateDocumentCommand(parentDocumentId, childrenDocumentIds.ToArray());
        await mediator.Send(relateDocumentCommand, cancellationToken);


        // Finalize the parent document
        // The parent document must not be finalized before the attachments has been set as children
        BackgroundJob.Enqueue<FinalizeDocument>(x => x.Run(parentDocumentId, CancellationToken.None));
    }
}