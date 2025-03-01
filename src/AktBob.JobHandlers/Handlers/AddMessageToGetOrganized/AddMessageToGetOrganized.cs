using AAK.GetOrganized.UploadDocument;
using AAK.GetOrganized;
using AktBob.Deskpro.Contracts.DTOs;
using System.Text;
using AktBob.CloudConvert.Contracts;
using AktBob.JobHandlers.Utils;
using AktBob.GetOrganized.Contracts;
using AktBob.Deskpro.Contracts;
using AktBob.Database.Contracts;

namespace AktBob.JobHandlers.Handlers.AddMessageToGetOrganized;
internal class AddMessageToGetOrganized(
    ILogger<AddMessageToGetOrganized> logger, IServiceScopeFactory serviceScopeFactory)
{
    private readonly ILogger<AddMessageToGetOrganized> _logger = logger;
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;

    public async Task Run(int deskproMessageId, string caseNumber, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(caseNumber))
        {
            _logger.LogError("DeskproMessageId {id}: Case number is null or empty", deskproMessageId);
            return;
        }

        using var scope = _serviceScopeFactory.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var cloudConvertHandlers = scope.ServiceProvider.GetRequiredService<ICloudConvertHandlers>();
        var deskproHelper = scope.ServiceProvider.GetRequiredService<DeskproHelper>();
        var deskproHandlers = scope.ServiceProvider.GetRequiredService<IDeskproHandlers>();
        var uploadGetOrganizedDocumentHandler = scope.ServiceProvider.GetRequiredService<IUploadGetOrganizedDocumentHandler>();

        try
        {
            var databaseMessage = await unitOfWork.Messages.GetByDeskproMessageId(deskproMessageId);

            if (databaseMessage is null)
            {
                _logger.LogError("Error getting databaese message by Deskpro message Id {id}", deskproMessageId);
                return;
            }

            // Get message from database, check if documentId is null
            if (databaseMessage.GODocumentId is not null)
            {
                // The message is already journalized -> do nothing
                return;
            }

            var databaseTicket = await unitOfWork.Tickets.Get(databaseMessage.TicketId);
            if (databaseTicket is null)
            {
                _logger.LogError("Database ticket {id} not found", databaseMessage.TicketId);
                return;
            }

            // Get Deskpro ticket (we need the deskpro ticket id to query the message ifself)
            var deskproTicketResult = await deskproHelper.GetTicket(deskproHandlers.GetDeskproTicket, databaseTicket.DeskproId, cancellationToken);
            if (!deskproTicketResult.IsSuccess)
            {
                _logger.LogError("Error getting Deskpro ticket {id}", databaseTicket.DeskproId);
                return;
            }

            var deskproTicket = deskproTicketResult.Value;

            // Get Deskpro message
            var getDeskproMessageResult = await deskproHandlers.GetDeskproMessage.Handle(databaseTicket.DeskproId, databaseMessage.DeskproMessageId, cancellationToken);

            if (!getDeskproMessageResult.IsSuccess)
            {
                _logger.LogError("Error requesting Deskpro message ID {id}. Message will be marked 'deleted' in database.", deskproMessageId);
                BackgroundJob.Enqueue<DeleteMessage>(x => x.Run(databaseMessage.Id, CancellationToken.None)); // Important: Use the database row ID here!
                return;
            }

            var deskproMessage = getDeskproMessageResult.Value;

            // Get Deskpro person
            var personResult = await deskproHelper.GetPerson(deskproHandlers.GetDeskproPerson, deskproMessage.Person.Id, cancellationToken);
            var person = personResult.Value;

            // Get attachments
            var attachments = Enumerable.Empty<AttachmentDto>();
            if (getDeskproMessageResult.Value.AttachmentIds.Any())
            {
                attachments = await deskproHelper.GetMessageAttachments(deskproHandlers.GetDeskproMessageAttachments, deskproTicket.Id, deskproMessage.Id, cancellationToken);
            }


            // Generate PDF document
            _logger.LogInformation("Generating PDF document from Deskpro message {id}", deskproMessageId);

            var generateDocumentResult = await GenerateDocument(cloudConvertHandlers, deskproMessage.CreatedAt, person.FullName, person.Email, deskproMessage.Content, caseNumber, deskproTicket.Subject, databaseMessage.MessageNumber ?? 0, attachments, cancellationToken);
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
            var uploadDocumentResult = await uploadGetOrganizedDocumentHandler.Handle(generateDocumentResult.Value, caseNumber, fileName, metadata, false, cancellationToken);
            if (!uploadDocumentResult.IsSuccess)
            {
                _logger.LogError("Error uploading document to GetOrganized: Deskpro message {messageId}, GO case '{goCaseNumber}'", deskproMessageId, caseNumber);
                return;
            }


            // Update database
            // TODO: improve this: We need call this directly here and not in a background job since adding the documentId to the message in the database prevents uploading the message again next time
            databaseMessage.GODocumentId = uploadDocumentResult.Value;
            await unitOfWork.Messages.Update(databaseMessage);

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


    private async Task<Result<byte[]>> GenerateDocument(ICloudConvertHandlers cloudConvertHandlers,
                                                        DateTime createdAt,
                                                        string personName,
                                                        string personEmail,
                                                        string content,
                                                        string caseNumber,
                                                        string caseTitle,
                                                        int messageNumber,
                                                        IEnumerable<AttachmentDto> attachments,
                                                        CancellationToken cancellationToken = default)
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
        var jobIdResult = await cloudConvertHandlers.ConvertHtmlToPdf.Handle([bytes], cancellationToken);

        if (!jobIdResult.IsSuccess)
        {
            // TODO
            return Result.Error();
        }

        var jobResult = await cloudConvertHandlers.GetCloudConvertJob.Handle(jobIdResult.Value, cancellationToken);

        if (!jobResult.IsSuccess)
        {
            return Result.Error();
        }

        return jobResult.Value;
    }
}