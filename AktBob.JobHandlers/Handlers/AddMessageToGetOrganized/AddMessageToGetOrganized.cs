using AAK.GetOrganized.UploadDocument;
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
using AktBob.Database.Contracts.Messages;

namespace AktBob.JobHandlers.Handlers.AddMessageToGetOrganized;
internal class AddMessageToGetOrganized(ILogger<AddMessageToGetOrganized> logger, IServiceScopeFactory serviceScopeFactory, DeskproHelper deskproHelpers)
{
    private readonly ILogger<AddMessageToGetOrganized> _logger = logger;
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;
    private readonly DeskproHelper _deskproHelpers = deskproHelpers;

    public async Task Run(int deskproMessageId, string caseNumber, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(caseNumber))
        {
            _logger.LogError("DeskproMessageId {id}: Case number is null or empty", deskproMessageId);
            return;
        }

        using var scope = _serviceScopeFactory.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        try
        {
            var getDatabaseMessageQuery = new GetMessageByDeskproMessageIdQuery(deskproMessageId);
            var getDatabaseMessageResult = await mediator.SendRequest(getDatabaseMessageQuery, cancellationToken);

            if (!getDatabaseMessageResult.IsSuccess || getDatabaseMessageResult.Value is null)
            {
                _logger.LogError("Error getting databaese message by Deskpro message Id {id}", deskproMessageId);
                return;
            }

            var databaseMessage = getDatabaseMessageResult.Value;
            var deskproTicketId = databaseMessage.DeskproTicketId;

            // Get message from database, check if documentId is null
            if (databaseMessage.GODocumentId is not null)
            {
                // The message is already journalized -> do nothing
                return;
            }
            
            // Get Deskpro ticket (we need the deskpro ticket id to query the message ifself)
            var deskproTicketResult = await _deskproHelpers.GetTicket(mediator, deskproTicketId);
            if (!deskproTicketResult.IsSuccess)
            {
                _logger.LogError("Error getting Deskpro ticket {id}", deskproTicketId);
                return;
            }

            var deskproTicket = deskproTicketResult.Value;

            // Get Deskpro message
            var getDeskproMessageQuery = new GetDeskproMessageByIdQuery(deskproTicketId, deskproMessageId);
            var getDeskproMessageResult = await mediator.SendRequest(getDeskproMessageQuery, cancellationToken);

            if (!getDeskproMessageResult.IsSuccess)
            {
                _logger.LogError("Error requesting Deskpro message ID {id}. Message will be marked 'deleted' in database.", deskproMessageId);
                BackgroundJob.Enqueue<DeleteMessage>(x => x.Run(databaseMessage.Id, CancellationToken.None)); // Important: Use the database row ID here!
                return;
            }

            var deskproMessage = getDeskproMessageResult.Value;

            // Get Deskpro person
            var personResult = await _deskproHelpers.GetPerson(mediator, deskproMessage.Person.Id);
            var person = personResult.Value;

            // Get attachments
            var attachments = Enumerable.Empty<AttachmentDto>();
            if (getDeskproMessageResult.Value.AttachmentIds.Any())
            {
                attachments = await _deskproHelpers.GetMessageAttachments(mediator, deskproTicket.Id, deskproMessage.Id);
            }


            // Generate PDF document
            _logger.LogInformation("Generating PDF document from Deskpro message {id}", deskproMessageId);

            var generateDocumentResult = await GenerateDocument(mediator, deskproMessage.CreatedAt, person.FullName, person.Email, deskproMessage.Content, caseNumber, deskproTicket.Subject, databaseMessage.MessageNumber ?? 0, attachments, cancellationToken);
            if (!generateDocumentResult.IsSuccess)
            {
                _logger.LogError("Error generating the message document for Deskpro message {id}", deskproMessageId);
                return;
            }


            DateTime createdAtDanishTime = getDeskproMessageResult!.Value.CreatedAt.UtcToDanish();
            var documentCategory = getDeskproMessageResult.Value.IsAgentNote ? DocumentCategory.Intern : MapDocumentCategoryFromPerson(personResult.Value);

            var metadata = new UploadDocumentMetadata
            {
                DocumentDate = createdAtDanishTime,
                DocumentCategory = documentCategory
            };

            var fileName = GenerateFileName(databaseMessage.MessageNumber ?? 0, person.FullName, createdAtDanishTime);


            // Upload parent document
            var uploadDocumentCommand = new UploadDocumentCommand(generateDocumentResult.Value, caseNumber, fileName, metadata, false);
            var uploadDocumentResult = await mediator.SendRequest(uploadDocumentCommand, cancellationToken);

            if (!uploadDocumentResult.IsSuccess)
            {
                _logger.LogError("Error uploading document to GetOrganized: Deskpro message {messageId}, GO case '{goCaseNumber}'", deskproMessageId, caseNumber);
                return;
            }


            // Update database
            // TODO: improve this: We need call this directly here and not in a background job since adding the documentId to the message in the database prevents uploading the message again next time
            var updateMessageCommand = new UpdateMessageSetGoDocumentIdCommand(deskproMessage.Id, uploadDocumentResult.Value);
            await mediator.Send(updateMessageCommand, cancellationToken);
            _logger.LogInformation("Database updated: GetOrganized documentId {documentId} set for message {id}", uploadDocumentResult.Value, deskproMessage.Id);

            if (attachments.Any())
            {
                // Handle message attachments
                // Note: the attachments handler also finalizing the parent document
                BackgroundJob.Enqueue<ProcessMessageAttachments>(x => x.UploadToGetOrganized(uploadDocumentResult.Value, caseNumber, deskproMessage.CreatedAt, documentCategory, attachments, CancellationToken.None));
            }
            else
            {
                // Finalize the parent document
                BackgroundJob.Enqueue<FinalizeDocument>(x => x.Run(uploadDocumentResult.Value, CancellationToken.None));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("{name}: {e}", nameof(AddMessageToGetOrganized), ex.Message);
        }
    }


    private static string GenerateFileName(int messageNumber, string personName, DateTime createdAtDanishTime)
    {
        // Using a list of strings to construct the title so we later can join them with a space separator.
        // Just a lazy way for not worry about space seperators manually...
        var titleElements = new List<string>
        {
            "Besked",
            $"({messageNumber.ToString("D3")})"
        };
        

        if (!string.IsNullOrEmpty(personName))
        {
            titleElements.Add(personName);
        }

        titleElements.Add($"({createdAtDanishTime.ToString("dd-MM-yyyy HH-mm-ss")}).pdf");
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


    private async Task<Result<byte[]>> GenerateDocument(IMediator mediator, DateTime createdAt, string personName, string personEmail,
        string content, string caseNumber, string caseTitle, int messageNumber,
        IEnumerable<AttachmentDto> attachments, CancellationToken cancellationToken = default)
    {

        var html = HtmlHelper.GenerateMessageHtml(
            createdAt: createdAt,
            personName: personName,
            personEmail: personEmail,
            content: content,
            caseNumber: caseNumber,
            caseTitle: caseTitle,
            messageNumber: messageNumber,
            attachments: attachments);

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