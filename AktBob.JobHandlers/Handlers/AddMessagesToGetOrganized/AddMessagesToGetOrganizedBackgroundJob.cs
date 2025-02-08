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
using Hangfire;

namespace AktBob.JobHandlers.Handlers.AddMessagesToGetOrganized;
internal class AddMessagesToGetOrganizedBackgroundJob(
    ILogger<AddMessagesToGetOrganizedBackgroundJob> logger,
    IServiceScopeFactory serviceScopeFactory,
    DeskproHelper deskproHelpers) : BackgroundService
{
    private readonly ILogger<AddMessagesToGetOrganizedBackgroundJob> _logger = logger;
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;
    private readonly DeskproHelper _deskproHelpers = deskproHelpers;

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
                _logger.LogError("Error requesting database for messages not journalized");
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
                var documentCategory = getDeskproMessageResult.Value.IsAgentNote ? DocumentCategory.Intern : MapDocumentCategoryFromPerson(personResult.Value);

                var metadata = new UploadDocumentMetadata
                {
                    DocumentDate = createdAtDanishTime,
                    DocumentCategory = documentCategory
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
                // TODO: improve this: We need call this directly here and not in a background job since adding the documentId to the message in the database prevents uploading the message again next time
                var updateMessageCommand = new UpdateMessageCommand(message.Id, uploadDocumentResult.Value);
                await mediator.Send(updateMessageCommand, stoppingToken);
                _logger.LogInformation("Database updated: GetOrganized documentId {documentId} set for message {id}", uploadDocumentResult.Value, message.Id);

                if (attachments.Any())
                {
                    // Handle message attachments
                    // Note: the attachments handler also finalizing the parent document
                    BackgroundJob.Enqueue<ProcessMessageAttachments>(x => x.UploadToGetOrganized(uploadDocumentResult.Value, message.GOCaseNumber, getDeskproMessageResult!.Value.CreatedAt, documentCategory, attachments, CancellationToken.None));
                }
                else
                {
                    // Finalize the parent document
                    BackgroundJob.Enqueue<FinalizeDocument>(x => x.Run(uploadDocumentResult.Value, CancellationToken.None));

                }
            }
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