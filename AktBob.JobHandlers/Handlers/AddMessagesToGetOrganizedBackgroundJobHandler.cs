using Microsoft.Extensions.Configuration;
using AAK.GetOrganized.UploadDocument;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using AAK.GetOrganized;
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
using AktBob.Database.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace AktBob.JournalizeDocuments.BackgroundServices;
internal class AddMessagesToGetOrganizedBackgroundJobHandler : BackgroundService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<AddMessagesToGetOrganizedBackgroundJobHandler> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly DeskproHelper _deskproHelpers;

    public AddMessagesToGetOrganizedBackgroundJobHandler(
        IConfiguration configuration,
        ILogger<AddMessagesToGetOrganizedBackgroundJobHandler> logger,
        IServiceScopeFactory serviceScopeFactory,
        DeskproHelper deskproHelpers)
    {
        _configuration = configuration;
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
        _deskproHelpers = deskproHelpers;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);

            // Get new messages from the database
            var getMessagesQueryResult = await mediator.SendRequest(new GetMessagesQuery(IncludeJournalized: false));

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
                var deskproTicketResult = await _deskproHelpers.GetDeskproTicket(mediator, message.DeskproTicketId);
                if (!deskproTicketResult.IsSuccess)
                {
                    _logger.LogError("Error getting Deskpro ticket {id}", message.DeskproTicketId);
                    continue;
                }


                // Get Deskpro message
                var getDeskproMessageQuery = new GetDeskproMessageByIdQuery(message.TicketId, message.DeskproMessageId);
                var getDeskproMessageResult = await mediator.SendRequest(getDeskproMessageQuery, stoppingToken);

                if (!getDeskproMessageResult.IsSuccess)
                {
                    _logger.LogError("Error requesting Deskpro message #{id}. Message will be marked as 'deleted' in database.", message.Id);

                    var deleteMessageCommand = new DeleteMessageCommand(message.Id);
                    await mediator.Send(deleteMessageCommand);
                    continue;
                }


                // Get Deskpro person
                var personResult = await _deskproHelpers.GetDeskproPerson(mediator, getDeskproMessageResult!.Value.Person.Id);


                // Get attachments
                var attachments = Enumerable.Empty<AttachmentDto>();
                if (getDeskproMessageResult.Value.AttachmentIds.Any())
                {
                    attachments = await _deskproHelpers.GetDeskproMessageAttachments(mediator, deskproTicketResult.Value!.Id, getDeskproMessageResult.Value.Id);
                }


                // Generate PDF document
                var generateDocumentResult = await GenerateDocument(mediator, message, deskproTicketResult.Value!, getDeskproMessageResult!, personResult.Value, attachments);
                if (!generateDocumentResult.IsSuccess)
                {
                    _logger.LogError("Error generating the message document for Deskpro message {id}", message.DeskproMessageId);
                    continue;
                }


                DateTime createdAtDanishTime = getDeskproMessageResult!.Value.CreatedAt.UtcToDanish();

                var metadata = new UploadDocumentMetadata
                {
                    DocumentDate = createdAtDanishTime,
                    DocumentCategory = getDeskproMessageResult.Value.IsAgentNote ? DocumentCategory.Intern : MapDocumentCategoryFromPerson(personResult.Value)
                };

                var fileName = GenerateFileName(message, personResult, createdAtDanishTime);


                // Upload parent document
                var uploadDocumentCommand = new UploadDocumentCommand(generateDocumentResult.Value, message.GOCaseNumber, fileName, metadata, false);
                var uploadDocumentResult = await mediator.SendRequest(uploadDocumentCommand, stoppingToken);

                if (!uploadDocumentResult.IsSuccess)
                {
                    _logger.LogError("Error uploading document to GetOrganized: Deskpro message {messageId}, GO case '{goCaseNumber}'", message.DeskproMessageId, message.GOCaseNumber);
                    continue;
                }


                // Update database
                var updateMessageCommand = new UpdateMessageCommand(message.Id, uploadDocumentResult.Value);
                await mediator.Send(updateMessageCommand);


                // Handle message attachments
                await ProcessAttachments(mediator, attachments, message.GOCaseNumber, metadata, uploadDocumentResult, stoppingToken);


                // Finalize the parent document
                // IMPORTANT: the parent document must not be finalized before the attachments has been set as children
                var finalizeParentDocumentCommand = new FinalizeDocumentCommand(uploadDocumentResult.Value, false);
                await mediator.Send(finalizeParentDocumentCommand, stoppingToken);
            }

        }
    }

    private async Task ProcessAttachments(IMediator mediator, IEnumerable<AttachmentDto> attachments, string caseNumber, UploadDocumentMetadata metadata, int? parentDocumentId, CancellationToken cancellationToken = default)
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
                var getAttachmentStreamResult = await mediator.SendRequest(getAttachmentStreamQuery, cancellationToken);

                if (!getAttachmentStreamResult.IsSuccess)
                {
                    _logger.LogError("Error downloading attachment '{filename}' from Deskpro message #{messageId}, ticketId {ticketId}", attachment.FileName, attachment.MessageId, attachment.TicketId);
                    continue;
                }

                getAttachmentStreamResult.Value.CopyTo(stream);
                var attachmentBytes = stream.ToArray();

                // Upload the attachment to GO
                var uploadDocumentCommand = new UploadDocumentCommand(attachmentBytes, caseNumber, attachment.FileName, metadata, false);
                var uploadDocumentResult = await mediator.SendRequest(uploadDocumentCommand, cancellationToken); // TODO: make unique filenames independent from possible file already uploaded with same file name
                if (!uploadDocumentResult.IsSuccess)
                {
                    continue;
                }

                // Finalize the attachment
                var finalizeDocumentCommand = new FinalizeDocumentCommand(uploadDocumentResult.Value, false);
                await mediator.Send(finalizeDocumentCommand, cancellationToken);
                childrenDocumentIds.Add(uploadDocumentResult.Value);
            }
        }

        // Set attachments as children
        if (childrenDocumentIds.Count > 0)
        {
            var relateDocumentCommand = new RelateDocumentCommand((int)parentDocumentId!, childrenDocumentIds.ToArray());
            await mediator.Send(relateDocumentCommand, cancellationToken);
        }
    }


    private static string GenerateFileName(Database.Contracts.Dtos.MessageDto message, PersonDto? person, DateTime createdAtDanishTime)
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


    private async Task<Result<byte[]>> GenerateDocument(IMediator mediator, Database.Contracts.Dtos.MessageDto databaseMessageDto, TicketDto deskproTicket, MessageDto deskproMessageDto, PersonDto? person, IEnumerable<AttachmentDto> attachmentDtos, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Generating PDF document from Deskpro message #{id}", deskproMessageDto.Id);

        var html = HtmlHelper.GenerateMessageHtml(deskproMessageDto, attachmentDtos, databaseMessageDto.GOCaseNumber ?? string.Empty, deskproTicket.Subject, databaseMessageDto.MessageNumber ?? 0);
        var bytes = Encoding.UTF8.GetBytes(html);

        var convertCommand = new ConvertHtmlToPdfCommand([bytes]);
        var convertResult = await mediator.SendRequest(convertCommand, cancellationToken);

        if (!convertResult.IsSuccess)
        {
            // TODO
            return Result.Error();
        }

        var jobQuery = new GetJobQuery(convertResult.Value.JobId);
        var jobResult = await mediator.SendRequest(jobQuery, cancellationToken);

        if (!jobResult.IsSuccess)
        {
            return Result.Error();
        }

        return jobResult.Value;
    }
}