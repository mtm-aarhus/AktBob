using Microsoft.Extensions.Configuration;
using AAK.GetOrganized.UploadDocument;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using AktBob.DatabaseAPI.Contracts.Queries;
using AAK.GetOrganized;
using AktBob.DatabaseAPI.Contracts.Commands;
using AktBob.Deskpro.Contracts.DTOs;
using Ardalis.Result;
using System.Text;
using AktBob.CloudConvert.Contracts;
using AktBob.Shared;
using MassTransit.Mediator;
using MassTransit;
using AktBob.JobHandlers.Utils;
using AktBob.GetOrganized.Contracts;
using AktBob.Deskpro.Contracts;

namespace AktBob.JournalizeDocuments.BackgroundServices;
internal class AddSingleMessagesToGetOrganizedBackgroundJobHandler : BackgroundService
{
    private readonly IMediator _mediator;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AddSingleMessagesToGetOrganizedBackgroundJobHandler> _logger;
    private readonly DeskproHelper _deskproHelpers;

    public AddSingleMessagesToGetOrganizedBackgroundJobHandler(
        IMediator mediator,
        IConfiguration configuration,
        ILogger<AddSingleMessagesToGetOrganizedBackgroundJobHandler> logger,
        DeskproHelper deskproHelpers)
    {
        _mediator = mediator;
        _configuration = configuration;
        _logger = logger;
        _deskproHelpers = deskproHelpers;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var delaySeconds = _configuration.GetValue<int?>("JournalizeDeskproMessages:WorkerIntervalSeconds") ?? 10;
        var journalizeAfterUpload = _configuration.GetValue<bool?>("JournalizeDeskproMessages:JournalizeAfterUpload") ?? true;

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromSeconds(delaySeconds), stoppingToken);

            try
            {
                // Get new messages from the database
                var getMessagesQueryResult = await _mediator.SendRequest(new GetMessagesNotJournalizedQuery());

                if (getMessagesQueryResult.Status == ResultStatus.NotFound)
                {
                    continue;
                }

                if (!getMessagesQueryResult.IsSuccess)
                {
                    _logger.LogError("Error requesting database API for messages not journalized");
                    continue;
                }


                // Loop through each messsage
                foreach (var message in getMessagesQueryResult.Value.OrderBy(m => m.DeskproMessageId))
                {
                    if (string.IsNullOrEmpty(message.GOCaseNumber))
                    {
                        // The Deskpro ticket that holds the message is not yet related to a GO-case -> do nothing
                        continue;
                    }

                    if (message.GODocumentId is not null)
                    {
                        // The message is already journalized -> do nothing
                        continue;
                    }


                    // From this point: the message is ready to be journalized


                    _logger.LogInformation("Deskpro message ready for upload to GetOrganized. DeskproTicketId {deskproTicketId}, DeskproMessageId {deskproMessageId}", message.DeskproTicketId, message.DeskproMessageId);


                    // Get Deskpro ticket
                    var deskproTicketResult = await _deskproHelpers.GetDeskproTicket(message.DeskproTicketId);
                    if (!deskproTicketResult.IsSuccess)
                    {
                        _logger.LogError("Error getting Deskpro ticket {id}", message.DeskproTicketId);
                        continue;
                    }


                    // Get Deskpro message
                    var deskproMessageResult = await _deskproHelpers.GetDeskproMessage(message.TicketId, message.DeskproMessageId);
                    if (!deskproMessageResult.IsSuccess)
                    {
                        _logger.LogError("Error requesting Deskpro message #{id}. Message will be marked as 'deleted' in database.", message.Id);

                        var deleteMessageCommand = new DeleteMessageCommand(message.Id);
                        await _mediator.Send(deleteMessageCommand);
                        continue;
                    }


                    // Get Deskpro person
                    var personResult = await _deskproHelpers.GetDeskproPerson(deskproMessageResult!.Value.Person.Id);


                    // Get attachments
                    var attachments = Enumerable.Empty<AttachmentDto>();
                    if (deskproMessageResult.Value.AttachmentIds.Any())
                    {
                        attachments = await _deskproHelpers.GetDeskproMessageAttachments(deskproTicketResult.Value!.Id, deskproMessageResult.Value.Id);
                    }


                    // Generate PDF document
                    var generateDocumentResult = await GenerateDocument(message, deskproTicketResult.Value!, deskproMessageResult!, personResult.Value, attachments);
                    if (!generateDocumentResult.IsSuccess)
                    {
                        _logger.LogError("Error generating the message document for Deskpro message {id}", message.DeskproMessageId);
                        continue;
                    }


                    DateTime createdAtDanishTime = deskproMessageResult!.Value.CreatedAt.UtcToDanish();

                    var metadata = new UploadDocumentMetadata
                    {
                        DocumentDate = createdAtDanishTime,
                        DocumentCategory = deskproMessageResult.Value.IsAgentNote ? DocumentCategory.Intern : MapDocumentCategoryFromPerson(personResult.Value)
                    };

                    var fileName = GenerateFileName(message, personResult, createdAtDanishTime);


                    // Upload parent document
                    var uploadDocumentCommand = new UploadDocumentCommand(generateDocumentResult.Value, message.GOCaseNumber, fileName, metadata, false);
                    var uploadDocumentResult = await _mediator.SendRequest(uploadDocumentCommand, stoppingToken);

                    if (!uploadDocumentResult.IsSuccess)
                    {
                        _logger.LogError("Error uploading document to GetOrganized: Deskpro message {messageId}, GO case '{goCaseNumber}'", message.DeskproMessageId, message.GOCaseNumber);
                        continue;
                    }


                    // Update database
                    var updateMessageCommand = new UpdateMessageSetGoDocumentIdCommand(message.Id, uploadDocumentResult.Value);
                    await _mediator.Send(updateMessageCommand);


                    // Handle message attachments
                    await ProcessAttachments(attachments, message.GOCaseNumber, metadata, uploadDocumentResult, stoppingToken);


                    // Finalize the parent document
                    // IMPORTANT: the parent document must not be finalized before the attachments has been set as children
                    var finalizeParentDocumentCommand = new FinalizeDocumentCommand(uploadDocumentResult.Value, false);
                    await _mediator.Send(finalizeParentDocumentCommand, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }
    }

    private async Task ProcessAttachments(IEnumerable<AttachmentDto> attachments, string caseNumber, UploadDocumentMetadata metadata, int? parentDocumentId, CancellationToken cancellationToken = default)
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
                var getAttachmentStreamResult = await _mediator.SendRequest(getAttachmentStreamQuery, cancellationToken);

                if (!getAttachmentStreamResult.IsSuccess)
                {
                    _logger.LogError("Error downloading attachment '{filename}' from Deskpro message #{messageId}, ticketId {ticketId}", attachment.FileName, attachment.MessageId, attachment.TicketId);
                    continue;
                }

                getAttachmentStreamResult.Value.CopyTo(stream);
                var attachmentBytes = stream.ToArray();

                // Upload the attachment to GO
                var uploadDocumentCommand = new UploadDocumentCommand(attachmentBytes, caseNumber, attachment.FileName, metadata, false);
                var uploadDocumentResult = await _mediator.SendRequest(uploadDocumentCommand, cancellationToken); // TODO: make unique filenames independent from possible file already uploaded with same file name
                if (!uploadDocumentResult.IsSuccess)
                {
                    continue;
                }

                // Finalize the attachment
                var finalizeDocumentCommand = new FinalizeDocumentCommand(uploadDocumentResult.Value, false);
                await _mediator.Send(finalizeDocumentCommand, cancellationToken);
                childrenDocumentIds.Add(uploadDocumentResult.Value);
            }
        }

        // Set attachments as children
        if (childrenDocumentIds.Count > 0)
        {
            var relateDocumentCommand = new RelateDocumentCommand((int)parentDocumentId!, childrenDocumentIds.ToArray());
            await _mediator.Send(relateDocumentCommand, cancellationToken);
        }
    }


    private static string GenerateFileName(DatabaseAPI.Contracts.DTOs.MessageDto message, PersonDto? person, DateTime createdAtDanishTime)
    {
        // Using a list of strings to construct the title so we later can join them with a space separator.
        // Just a lazy way for not worry about space seperators manually...
        var titleElements = new List<string>
        {
            "Besked"
        };

        if (message.MessageNumber.HasValue)
        {
            titleElements.Add($"({message.MessageNumber?.ToString("D3")})");
        }

        if (person is not null)
        {
            titleElements.Add(person.FullName);
        }

        titleElements.Add($"({createdAtDanishTime.ToString("dd-MM-yyyy HH.mm.ss")}).pdf");
        var title = string.Join(" ", titleElements);

        return title;
    }


    private DocumentCategory MapDocumentCategoryFromPerson(PersonDto? person)
    {
        if (person is null)
        {
            return DocumentCategory.Intern;
        }

        return person.IsAgent ? DocumentCategory.Udgående : DocumentCategory.Indgående;
    }


    private async Task<Result<byte[]>> GenerateDocument(DatabaseAPI.Contracts.DTOs.MessageDto databaseMessageDto, TicketDto deskproTicket, MessageDto deskproMessageDto, PersonDto? person, IEnumerable<AttachmentDto> attachmentDtos, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Generating PDF document from Deskpro message #{id}", deskproMessageDto.Id);

        var html = HtmlHelper.GenerateMessageHtml(deskproMessageDto, attachmentDtos, databaseMessageDto.GOCaseNumber ?? string.Empty, deskproTicket.Subject, databaseMessageDto.MessageNumber ?? 0);
        var bytes = Encoding.UTF8.GetBytes(html);

        var convertCommand = new ConvertHtmlToPdfCommand([bytes]);
        var convertResult = await _mediator.SendRequest(convertCommand, cancellationToken);

        if (!convertResult.IsSuccess)
        {
            // TODO
            return Result.Error();
        }

        var jobQuery = new GetJobQuery(convertResult.Value.JobId);
        var jobResult = await _mediator.SendRequest(jobQuery, cancellationToken);
        
        if (!jobResult.IsSuccess)
        {
            return Result.Error();
        }

        return jobResult.Value;
    }
}