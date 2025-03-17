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
        if (databaseMessage is null) throw new BusinessException("Unable to get message from database.");

        // Get message from database, check if documentId is null
        if (databaseMessage.GODocumentId is not null)
        {
            _logger.LogDebug("Message in database already has a value for {propertyName}. Exiting job.", nameof(databaseMessage.GODocumentId));
            return;
        }

        var databaseTicket = await unitOfWork.Tickets.Get(databaseMessage.TicketId);
        if (databaseTicket is null) throw new BusinessException($"Unable to get ticket {databaseMessage.TicketId} from database.");

        // Get Deskpro ticket (we need the deskpro ticket id to query the message ifself)
        var deskproTicketResult = await deskpro.GetTicket(databaseTicket.DeskproId, cancellationToken);
        if (!deskproTicketResult.IsSuccess) throw new BusinessException("Unable to get ticket {id} from Deskpro.");
        var deskproTicket = deskproTicketResult.Value;

        // Get Deskpro message
        var getDeskproMessageResult = await deskpro.GetMessage(databaseTicket.DeskproId, databaseMessage.DeskproMessageId, cancellationToken);
        if (!getDeskproMessageResult.IsSuccess) throw new BusinessException("Unable to get message from Deskpro. Please mark message as deleted to avoid future processing failure.");
        var deskproMessage = getDeskproMessageResult.Value;

        // Get Deskpro person
        var personResult = await deskpro.GetPerson(deskproMessage.Person.Id, cancellationToken);
        var person = personResult.Value;

        // Get recipient
        var recipient = deskproMessage.Recipients.FirstOrDefault() != null && !deskproMessage.CreationSystem.Equals("web.api")
            ? await deskpro.GetPerson(deskproMessage.Recipients.First(), cancellationToken)
            : Result<PersonDto>.Error();

        // Get attachments
        var attachments = Enumerable.Empty<AttachmentDto>();
        if (getDeskproMessageResult.Value.AttachmentIds.Any())
        {
            var getAttachmentsResult = await deskpro.GetMessageAttachments(deskproTicket.Id, deskproMessage.Id, cancellationToken);
            attachments = getAttachmentsResult.Value ?? Enumerable.Empty<AttachmentDto>();
        }

        // Generate PDF document
        var generateDocumentResult = await GenerateDocument(
            cloudConvert,
            deskproMessage.CreatedAt,
            person.FullName,
            person.Email,
            recipient.Value?.FullName ?? string.Empty,
            recipient.Value?.Email ?? deskproMessage.Recipients.FirstOrDefault() ?? string.Empty,
            deskproMessage.Content,
            job.CaseNumber,
            deskproTicket.Subject,
            databaseMessage.MessageNumber ?? 0,
            attachments,
            deskproMessage.IsAgentNote,
            cancellationToken);
        if (!generateDocumentResult.IsSuccess) throw new BusinessException($"Unable to generate PDF document using CloudConvert: {generateDocumentResult.Errors.AsString()}");

        // Upload parent document
        DateTime createdAtDanishTime = getDeskproMessageResult!.Value.CreatedAt.UtcToDanish();
        var documentCategory = getDeskproMessageResult.Value.IsAgentNote ? UploadDocumentCategory.Internal : MapDocumentCategoryFromPerson(personResult.Value);
        var fileName = GenerateFileName(databaseMessage.MessageNumber ?? 0, person.FullName, createdAtDanishTime);

        var upoadDocumentCommand = new UploadDocumentCommand(
            generateDocumentResult.Value,
            job.CaseNumber,
            fileName,
            string.Empty,
            createdAtDanishTime,
            documentCategory,
            false);

        var uploadedDocumentIdResult = await getOrganized.UploadDocument(upoadDocumentCommand, cancellationToken);
        if (!uploadedDocumentIdResult.IsSuccess) throw new BusinessException("Unable to upload the message PDF document to GetOrganized");

        // Update database
        databaseMessage.GODocumentId = uploadedDocumentIdResult.Value;
        if (!await unitOfWork.Messages.Update(databaseMessage)) throw new BusinessException($"Unable to update database message ID {databaseMessage.Id} setting GODocumentId = {uploadedDocumentIdResult.Value}");

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
                                                        string recipientName,
                                                        string recipientEmail,
                                                        string content,
                                                        string caseNumber,
                                                        string caseTitle,
                                                        int messageNumber,
                                                        IEnumerable<AttachmentDto> attachments,
                                                        bool isAgentNote,
                                                        CancellationToken cancellationToken = default)
    {
        var html = HtmlHelper.GenerateMessageHtml(
            isAgentNote: isAgentNote,
            createdAt: createdAt,
            personName: personName,
            personEmail: personEmail,
            recipientName: recipientName,
            recipientEmail: recipientEmail,
            content: content,
            caseNumber: caseNumber,
            caseTitle: caseTitle,
            messageNumber: messageNumber,
            attachments: attachments);

        var bytes = Encoding.UTF8.GetBytes(html);

        var generateTasksResult = cloudConvertModule.GenerateTasks([bytes]);
        if (!generateTasksResult.IsSuccess)
        {
            return Result.Error("Failed generating tasks.");
        }

        var jobIdResult = await cloudConvertModule.ConvertHtmlToPdf(generateTasksResult.Value, cancellationToken);
        if (!jobIdResult.IsSuccess)
        {
            return Result.Error("Failed converting HTML to PDF.");
        }

        var getUrlResult = await cloudConvertModule.GetDownloadUrl(jobIdResult.Value, cancellationToken);
        if (!getUrlResult.IsSuccess || string.IsNullOrEmpty(getUrlResult))
        {
            return Result.Error("Failed to get download url.");
        }

        var fileResult = await cloudConvertModule.DownloadFile(getUrlResult, cancellationToken);
        if (!fileResult.IsSuccess)
        {
            return Result.Error($"Failed to download file: {getUrlResult.Value}");
        }

        return fileResult.Value;
    }

}