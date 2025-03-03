using AAK.GetOrganized;
using AAK.GetOrganized.UploadDocument;
using AktBob.CloudConvert.Contracts;
using AktBob.Database.Contracts;
using AktBob.Deskpro.Contracts;
using AktBob.Deskpro.Contracts.DTOs;
using AktBob.GetOrganized.Contracts;
using AktBob.JobHandlers.Utils;
using AktBob.Shared.Jobs;
using System.Text;

namespace AktBob.JobHandlers.Handlers.AddOrUpdateDeskproTicketToGetOrganized;
internal class AddOrUpdateDeskproTicketToGetOrganized(ILogger<AddOrUpdateDeskproTicketToGetOrganized> logger, IServiceScopeFactory serviceScopeFactory) : IJobHandler<AddOrUpdateDeskproTicketToGetOrganizedJob>
{
    private readonly ILogger<AddOrUpdateDeskproTicketToGetOrganized> _logger = logger;
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;

    public async Task Handle(AddOrUpdateDeskproTicketToGetOrganizedJob job, CancellationToken cancellationToken = default)
    {
        var scope = _serviceScopeFactory.CreateScope();
        var pendingsTickets = scope.ServiceProvider.GetRequiredService<PendingsTickets>();

        var deskproHandlers = scope.ServiceProvider.GetRequiredService<IDeskproHandlers>();
        var deskproModule = scope.ServiceProvider.GetRequiredService<IDeskproModule>();
        var deskproHelper = scope.ServiceProvider.GetRequiredService<DeskproHelper>();
        var messageRepository = scope.ServiceProvider.GetRequiredService<IMessageRepository>();
        var cloudConvertModule = scope.ServiceProvider.GetRequiredService<ICloudConvertModule>();
        var uploadGetOrganizedDocumentHandler = scope.ServiceProvider.GetRequiredService<IUploadGetOrganizedDocumentHandler>();
        var currentPendingTicket = new PendingTicket(job.TicketId, job.SubmittedAt);

        pendingsTickets.AddPendingTicket(currentPendingTicket);

        // Check if this submission is the most recent for the specified ticket
        if (!IsMostRecentSubmission(currentPendingTicket, pendingsTickets))
        {
            return;
        }

        List<byte[]> content = new();


        // Get Deskpro Ticket
        var ticketResult = await deskproHelper.GetTicket(deskproHandlers.GetDeskproTicket, job.TicketId, cancellationToken);

        if (!ticketResult.IsSuccess || ticketResult.Value is null)
        {
            _logger.LogError("Error getting ticket {id} from Deskpro", job.TicketId);
            return;
        }

        var ticket = ticketResult.Value;
        

        // Get Deskpro custom fields specification
        var ticketCustomFieldsResult = await deskproModule.GetCustomFieldSpecifications(cancellationToken); // TODO: Cache
        if (!ticketCustomFieldsResult.IsSuccess)
        {
            _logger.LogError("Error getting custom fields specifications from Deskpro");
            return;
        }

        // Get Deskpro ticket agent
        var agentResult = await deskproHelper.GetPerson(deskproHandlers.GetDeskproPerson, ticket.Agent?.Id ?? 0, cancellationToken);

        // Get Deskpro ticket user
        var userResult = await deskproHelper.GetPerson(deskproHandlers.GetDeskproPerson, ticket.Person?.Id ?? 0, cancellationToken);

        // Map ticket fields
        var customFields = GenerateCustomFieldValues(job.CustomFieldIds, ticketCustomFieldsResult.Value, ticket);
        var caseNumbers = HtmlHelper.GenerateListOfFieldValues(job.CaseNumberFieldIds, ticket, "ticket-case-numbers.html");

        var ticketDictionary = new Dictionary<string, string>
        {
            { "ticketId", ticket.Id.ToString() },
            { "caseTitle", ticket.Subject },
            { "userName", userResult.Value?.FullName ?? string.Empty },
            { "userEmail", userResult.Value?.Email ?? string.Empty },
            { "userPhone", string.Join(", ", userResult.Value?.PhoneNumbers ?? Enumerable.Empty<string>()) },
            { "agentName", agentResult.Value?.FullName ?? string.Empty},
            { "agentEmail", agentResult.Value ?.Email ?? string.Empty },
            { "custom-fields", string.Join("", customFields) },
            { "caseNumbers", string.Join("", caseNumbers) }
        };

        var ticketHtml = HtmlHelper.GenerateHtml("ticket.html", ticketDictionary);
        content.Add(Encoding.UTF8.GetBytes(ticketHtml));


        // Messages
        var getMessagesResult = await deskproHandlers.GetDeskproMessages.Handle(ticket.Id, cancellationToken);

        if (getMessagesResult.IsSuccess)
        {
            var messages = getMessagesResult.Value.OrderByDescending(x => x.CreatedAt);
            foreach (var message in messages)
            {
                var person = await deskproHelper.GetPerson(deskproHandlers.GetDeskproPerson, message.Person?.Id ?? 0, cancellationToken);
                message.Person = person.Value;

                var attachments = Enumerable.Empty<AttachmentDto>();
                if (message.AttachmentIds.Any())
                {
                    attachments = await deskproHelper.GetMessageAttachments(deskproHandlers.GetDeskproMessageAttachments, ticket.Id, message.Id, cancellationToken);
                }

                // Get message number from API database
                var messageNumber = 0;
                var databaseMessage = await messageRepository.GetByDeskproMessageId(message.Id);

                if (databaseMessage is null)
                {
                    _logger.LogWarning("No message found in database for Deskpro message ID {id}", message.Id);
                }
                else
                {
                    messageNumber = databaseMessage.MessageNumber ?? 0;
                }

                var messageHtml = HtmlHelper.GenerateMessageHtml(message.CreatedAt, message.Person.FullName, message.Person.Email, message.Content, job.GOCaseNumber, ticket.Subject, messageNumber, attachments);
                content.Add(Encoding.UTF8.GetBytes(messageHtml));
            }
        }



        // Generate PDF
        var generateTasksResult = cloudConvertModule.GenerateTasks(content);
        if (!generateTasksResult.IsSuccess)
        {
            _logger.LogError("Error generating CloudConvert tasks dictionary for Deskpro ticket {id}", job.TicketId);
            return;
        }

        var jobIdResult = await cloudConvertModule.ConvertHtmlToPdf(generateTasksResult.Value, cancellationToken);
        if (!jobIdResult.IsSuccess)
        {
            _logger.LogError("Error creating CloudConvert job generating PDF for Deskpro ticket {id}", job.TicketId);
            return;
        }

        var urlResult = await cloudConvertModule.GetDownloadUrl(jobIdResult.Value, cancellationToken);
        if (!urlResult.IsSuccess)
        {
            _logger.LogError("Error querying job '{id}' from PDF from CloudConvert", jobIdResult.Value);
            return;
        }

        var fileResult = await cloudConvertModule.GetFile(urlResult.Value, cancellationToken);
        if (!fileResult.IsSuccess)
        {
            _logger.LogError("CloudConvert job {id}: Error downloading file from url: {url}", jobIdResult.Value, urlResult.Value);
            return;
        }

        // Check if this submission is the most recent for the specified ticket. Check this as late as possible.
        if (!IsMostRecentSubmission(currentPendingTicket, pendingsTickets))
        {
            return;
        }


        // Upload to GO
        var metadata = new UploadDocumentMetadata
        {
            DocumentDate = DateTime.UtcNow.UtcToDanish(),
            DocumentCategory = DocumentCategory.Intern
        };

        var fileName = "Samlet korrespondance.pdf";
        var uploadDocumentResult = await uploadGetOrganizedDocumentHandler.Handle(fileResult.Value, job.GOCaseNumber, fileName, metadata, true, cancellationToken);

        if (!uploadDocumentResult.IsSuccess)
        {
            _logger.LogError("Error uploading full ticket document to GetOrganized: Deskpro ticket {ticketId}, GO case '{goCaseNumber}'", job.TicketId, job.GOCaseNumber);
        }
        
        pendingsTickets.RemovePendingTicket(currentPendingTicket);
    }

    private bool IsMostRecentSubmission(PendingTicket pendingTicket, PendingsTickets pendingsTickets)
    {
        if (!pendingsTickets.IsMostRecent(pendingTicket))
        {
            _logger.LogInformation("Not the most current submission for updating the Deskpro PDF document. (Deskpro ticket {id}, submittedAt {submittedAt})", pendingTicket.TicketId, pendingTicket.SubmittedAt);
            pendingsTickets.RemovePendingTicket(pendingTicket);
            return false;
        }

        return true;
    }

    private IEnumerable<string> GenerateCustomFieldValues(int[] customFieldIds, IEnumerable<CustomFieldSpecificationDto> customFieldSpecificationDtos, TicketDto ticketDto)
    {
        List<string> items = new();

        foreach (var customFieldId in customFieldIds)
        {
            var title = customFieldSpecificationDtos.FirstOrDefault(c => c.Id == customFieldId)?.Title ?? string.Empty;
            var values = ticketDto.Fields.FirstOrDefault(f => f.Id == customFieldId)?.Values ?? Enumerable.Empty<string>();
            var value = string.Join(",", values);

            var dictionary = new Dictionary<string, string>
                {
                    { "title", title },
                    { "value", value }
                };

            var html = HtmlHelper.GenerateHtml("custom-field.html", dictionary);
            items.Add(html);
        }

        return items;
    }
}
