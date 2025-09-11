using System.Text;
using AktBob.GetOrganized.Contracts;
using AktBob.Database.Contracts;
using AktBob.Shared.Extensions;
using AktBob.CloudConvert.Contracts;
using Aktbob.Modules.Deskpro.Features.GetMessage;
using Aktbob.Modules.Deskpro.Features.GetMessageAttachments;
using Aktbob.Modules.Deskpro.Features.GetPersonByEmail;
using Aktbob.Modules.Deskpro.Features.GetPersonById;
using Aktbob.Modules.Deskpro.Features.GetTicket;
using AktBob.Shared.Contracts.Modules.Deskpro.DTOs;

namespace AktBob.Workflows.Processes.AddMessageToGetOrganized;

internal record AddMessageToGetOrganizedJob(int TicketId, int MessageId, string CaseNumber);

internal class AddMessageToGetOrganized(
    ILogger<AddMessageToGetOrganized> logger,
    IServiceScopeFactory serviceScopeFactory) : IJobHandler<AddMessageToGetOrganizedJob>
{
    private readonly ILogger<AddMessageToGetOrganized> _logger = logger;
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;

    public async Task Handle(AddMessageToGetOrganizedJob job, CancellationToken cancellationToken = default)
    {
        Guard.Against.NullOrEmpty(job.CaseNumber);
        
        _logger.LogInformation("Adding document to GetOrganized for ticket {ticket} message {message}.", job.TicketId, job.MessageId);
        
        using var scope = _serviceScopeFactory.CreateScope();
        var deskproGetTicketHandler = scope.ServiceProvider.GetRequiredService<IGetTicketHandler>();
        var deskproGetMessageHandler = scope.ServiceProvider.GetRequiredService<IGetMessageHandler>();
        var deskproGetPersonByIdHandler = scope.ServiceProvider.GetRequiredService<IGetPersonByIdHandler>();
        var deskproGetPersonByEmailHandler = scope.ServiceProvider.GetRequiredService<IGetPersonByEmailHandler>();
        var deskproGetMessageAttachmentsHandler = scope.ServiceProvider.GetRequiredService<IGetMessageAttachmentsHandler>();
        
        var cloudConvert = scope.ServiceProvider.GetRequiredServiceOrThrow<ICloudConvertModule>();
        var getOrganized = scope.ServiceProvider.GetRequiredServiceOrThrow<IGetOrganizedModule>();
        var jobDispatcher = scope.ServiceProvider.GetRequiredServiceOrThrow<IJobDispatcher>();
        var unitOfWork = scope.ServiceProvider.GetRequiredServiceOrThrow<IUnitOfWork>();

        var databaseMessage = await unitOfWork.Messages.GetByDeskproMessageId(job.MessageId);
        if (databaseMessage is null) throw new BusinessException($"Error adding document to GetOrganized: Unable to get ticket {job.TicketId} message {job.MessageId} from database.");

        // Get message from database, check if documentId is null
        if (databaseMessage.GODocumentId is not null)
        {
            _logger.LogDebug("GetOrganized document already exists for Deskpro ticket {ticketId} message {message}.", job.TicketId, job.MessageId);
            return;
        }

        var databaseTicket = await unitOfWork.Tickets.Get(databaseMessage.TicketId);
        if (databaseTicket is null) throw new BusinessException($"Error adding document to GetOrganized: Unable to get ticket {databaseMessage.TicketId} from database.");

        // Get Deskpro ticket (we need the deskpro ticket id to query the message ifself)
        var deskproTicketResult = await deskproGetTicketHandler.Handle(databaseTicket.DeskproId, cancellationToken);
        if (deskproTicketResult.IsError) throw new BusinessException($"Error adding document to GetOrganized: Unable to get ticket {databaseTicket.DeskproId} from Deskpro.");
        var deskproTicket = deskproTicketResult.Value;

        // Get Deskpro message
        var getDeskproMessageResult = await deskproGetMessageHandler.Handle(job.TicketId, job.MessageId, cancellationToken);
        if (getDeskproMessageResult.IsError) throw new BusinessException("rror adding document to GetOrganized: Unable to get message from Deskpro. Mark message in database as deleted to avoid future failures.");
        var deskproMessage = getDeskproMessageResult.Value;

        // Get Deskpro person
        var personResult = await deskproGetPersonByIdHandler.Handle(deskproMessage.Person.Id, cancellationToken);
        var person = personResult.Value;

        // Get recipient
        var recipient = deskproMessage.Recipients.FirstOrDefault() != null && !deskproMessage.CreationSystem.Equals("web.api")
            ? await deskproGetPersonByEmailHandler.Handle(deskproMessage.Recipients.First(), cancellationToken)
            : Error.NotFound().ToErrorOr<PersonDto>();

        // Get attachments
        var attachments = Enumerable.Empty<AttachmentDto>();
        if (getDeskproMessageResult.Value.AttachmentIds.Any())
        {
            var getAttachmentsResult = await deskproGetMessageAttachmentsHandler.Handle(job.TicketId, job.MessageId, cancellationToken);
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
        
        if (generateDocumentResult.IsError) throw new BusinessException($"Unable to generate PDF document using CloudConvert: {generateDocumentResult.Errors.ToCommaDelimitedString()}");

        // Upload parent document
        DateTime createdAtDanishTime = deskproMessage.CreatedAt.UtcToDanish();
        var documentCategory = deskproMessage.IsAgentNote ? UploadDocumentCategory.Internal : MapDocumentCategoryFromPerson(personResult.Value);
        var fileName = GenerateFileName(databaseMessage.MessageNumber ?? 0, person.FullName, createdAtDanishTime);

        var uploadedDocumentIdResult = await getOrganized.UploadDocument(
            bytes: generateDocumentResult.Value,
            caseNumber: job.CaseNumber,
            fileName: fileName,
            customProperty: string.Empty,
            documentDate: createdAtDanishTime,
            category: documentCategory,
            overwriteExisting: false,
            cancellationToken: cancellationToken);
        
        if (uploadedDocumentIdResult.IsError) throw new BusinessException(uploadedDocumentIdResult.Errors.ToCommaDelimitedString());

        // Update database
        databaseMessage.GODocumentId = uploadedDocumentIdResult.Value;
        if (!await unitOfWork.Messages.Update(databaseMessage)) throw new BusinessException($"Unable to update database message ID {databaseMessage.Id} setting GODocumentId = {uploadedDocumentIdResult.Value}");

        if (attachments.Any())
        {
            // Handle message attachments
            // Note: the attachments handler also finalizing the parent document
            jobDispatcher.Dispatch(new ProcessMessageAttachmentsJob(uploadedDocumentIdResult.Value, job.CaseNumber, deskproMessage.CreatedAt, documentCategory, attachments));
        }
        // else
        // {
        //     // Finalize the parent document
        //     getOrganized.FinalizeDocument(uploadedDocumentIdResult.Value, false);
        // }
        
        _logger.LogInformation("Document added to GetOrganized for ticket {ticket} message {message}.", job.TicketId, job.MessageId);
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


    private async Task<ErrorOr<byte[]>> GenerateDocument(ICloudConvertModule cloudConvertModule,
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
            createdAt: createdAt.UtcToDanish(),
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
        if (generateTasksResult.IsError)
        {
            return generateTasksResult.Errors;
        }

        var jobIdResult = await cloudConvertModule.ConvertHtmlToPdf(generateTasksResult.Value, cancellationToken);
        if (jobIdResult.IsError)
        {
            return jobIdResult.Errors;
        }

        var getUrlResult = await cloudConvertModule.GetDownloadUrl(jobIdResult.Value, cancellationToken);
        if (getUrlResult.IsError|| string.IsNullOrEmpty(getUrlResult.Value))
        {
            return getUrlResult.Errors;
        }

        var fileResult = await cloudConvertModule.DownloadFile(getUrlResult.Value, cancellationToken);
        if (fileResult.IsError)
        {
            return fileResult.Errors;
        }

        return fileResult.Value;
    }

}