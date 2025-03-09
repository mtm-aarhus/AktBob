using AktBob.Deskpro.Contracts.DTOs;
using System.Text;
using AktBob.CloudConvert.Contracts;
using AktBob.GetOrganized.Contracts;
using AktBob.Deskpro.Contracts;
using AktBob.Database.Contracts;
using AktBob.Shared.Extensions;
using AktBob.Workflows.Helpers;

namespace AktBob.Workflows.Processes.AddMessageToGetOrganized;

internal record AddMessageToGetOrganizedJob(int DeskproMessageId, string CaseNumber);

internal class AddMessageToGetOrganized(ILogger<AddMessageToGetOrganized> logger, IServiceScopeFactory serviceScopeFactory) : IJobHandler<AddMessageToGetOrganizedJob>
{
    private readonly ILogger<AddMessageToGetOrganized> _logger = logger;
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;

    public async Task Handle(AddMessageToGetOrganizedJob job, CancellationToken cancellationToken = default)
    {
        // Validate job parameters
        Guard.Against.NullOrEmpty(job.CaseNumber);
        Guard.Against.Zero(job.DeskproMessageId);

        using var scope = _serviceScopeFactory.CreateScope();
        var deskpro = scope.ServiceProvider.GetRequiredService<IDeskproModule>();
        var cloudConvert = scope.ServiceProvider.GetRequiredService<ICloudConvertModule>();
        var getOrganized = scope.ServiceProvider.GetRequiredService<IGetOrganizedModule>();
        var jobDispatcher = scope.ServiceProvider.GetRequiredService<IJobDispatcher>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var databaseMessage = await unitOfWork.Messages.GetByDeskproMessageId(job.DeskproMessageId);

        if (databaseMessage is null)
        {
            _logger.LogError("Error getting database message by Deskpro message Id {id}", job.DeskproMessageId);
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
        var deskproTicketResult = await deskpro.GetTicket(databaseTicket.DeskproId, cancellationToken);
        if (!deskproTicketResult.IsSuccess)
        {
            _logger.LogError("Error getting Deskpro ticket {id}", databaseTicket.DeskproId);
            return;
        }

        var deskproTicket = deskproTicketResult.Value;

        // Get Deskpro message
        var getDeskproMessageResult = await deskpro.GetMessage(databaseTicket.DeskproId, databaseMessage.DeskproMessageId, cancellationToken);

        if (!getDeskproMessageResult.IsSuccess)
        {
            _logger.LogError("Error requesting Deskpro message ID {id}. Message will be marked 'deleted' in database.", job.DeskproMessageId);
            await unitOfWork.Messages.Delete(databaseMessage.Id); // Important: Use the database row ID here!
            return;
        }

        var deskproMessage = getDeskproMessageResult.Value;

        // Get Deskpro person
        var personResult = await deskpro.GetPerson(deskproMessage.Person.Id, cancellationToken);
        var person = personResult.Value;

        // Get attachments
        var attachments = Enumerable.Empty<AttachmentDto>();
        if (getDeskproMessageResult.Value.AttachmentIds.Any())
        {
            var getAttachmentsResult = await deskpro.GetMessageAttachments(deskproTicket.Id, deskproMessage.Id, cancellationToken);
            attachments = getAttachmentsResult.Value ?? Enumerable.Empty<AttachmentDto>();
        }


        // Generate PDF document
        _logger.LogInformation("Generating PDF document from Deskpro message {id}", job.DeskproMessageId);

        var generateDocumentResult = await GenerateDocument(cloudConvert, deskproMessage.CreatedAt, person.FullName, person.Email, deskproMessage.Content, job.CaseNumber, deskproTicket.Subject, databaseMessage.MessageNumber ?? 0, attachments, cancellationToken);
        if (!generateDocumentResult.IsSuccess)
        {
            _logger.LogError("Error generating the message document for Deskpro message {id}", job.DeskproMessageId);
            return;
        }


        DateTime createdAtDanishTime = getDeskproMessageResult!.Value.CreatedAt.UtcToDanish();
        var documentCategory = getDeskproMessageResult.Value.IsAgentNote ? UploadDocumentCategory.Internal : MapDocumentCategoryFromPerson(personResult.Value);
        var fileName = GenerateFileName(databaseMessage.MessageNumber ?? 0, person.FullName, createdAtDanishTime);

        // Upload parent document
        var upoadDocumentCommand = new UploadDocumentCommand(
            generateDocumentResult.Value,
            job.CaseNumber,
            fileName,
            string.Empty,
            createdAtDanishTime,
            documentCategory,
            false);

        var uploadedDocumentIdResult = await getOrganized.UploadDocument(upoadDocumentCommand, cancellationToken);
        if (!uploadedDocumentIdResult.IsSuccess)
        {
            _logger.LogError("Error uploading document to GetOrganized: Deskpro message {messageId}, GO case '{goCaseNumber}'", job.DeskproMessageId, job.CaseNumber);
            return;
        }


        // Update database
        // TODO: improve this: We need call this directly here and not in a background job since adding the documentId to the message in the database prevents uploading the message again next time
        databaseMessage.GODocumentId = uploadedDocumentIdResult.Value;
        await unitOfWork.Messages.Update(databaseMessage);

        _logger.LogInformation("Database updated: GetOrganized documentId {documentId} set for message {id}", uploadedDocumentIdResult.Value, deskproMessage.Id);

        if (attachments.Any())
        {
            // Handle message attachments
            // Note: the attachments handler also finalizing the parent document
            jobDispatcher.Dispatch(new ProcessMessageAttachmentsJob(uploadedDocumentIdResult.Value, job.CaseNumber, deskproMessage.CreatedAt, documentCategory, attachments));
        }
        else
        {
            // Finalize the parent document
            var finalizeDocumentCommand = new FinalizeDocumentCommand(uploadedDocumentIdResult.Value);
            getOrganized.FinalizeDocument(finalizeDocumentCommand);
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


    private UploadDocumentCategory MapDocumentCategoryFromPerson(PersonDto? person)
    {
        if (person is null)
        {
            return UploadDocumentCategory.Internal;
        }

        return person.IsAgent ? UploadDocumentCategory.Outgoing : UploadDocumentCategory.Incoming;
    }


    private async Task<Result<byte[]>> GenerateDocument(ICloudConvertModule cloudConvertModule,
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

        var generateTasksResult = cloudConvertModule.GenerateTasks([bytes]);
        if (!generateTasksResult.IsSuccess)
        {
            return Result.Error();
        }

        var jobIdResult = await cloudConvertModule.ConvertHtmlToPdf(generateTasksResult.Value, cancellationToken);
        if (!jobIdResult.IsSuccess)
        {
            // TODO
            return Result.Error();
        }

        var getUrlResult = await cloudConvertModule.GetDownloadUrl(jobIdResult.Value, cancellationToken);
        if (!getUrlResult.IsSuccess || string.IsNullOrEmpty(getUrlResult))
        {
            return Result.Error();
        }

        var fileResult = await cloudConvertModule.GetFile(getUrlResult, cancellationToken);
        if (!fileResult.IsSuccess)
        {
            return Result.Error();
        }

        return fileResult.Value;
    }

}