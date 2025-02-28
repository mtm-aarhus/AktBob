using AAK.GetOrganized;
using AAK.GetOrganized.UploadDocument;
using AktBob.CloudConvert.Contracts;
using AktBob.Database.Contracts.Messages;
using AktBob.Deskpro.Contracts;
using AktBob.Deskpro.Contracts.DTOs;
using AktBob.GetOrganized.Contracts;
using AktBob.JobHandlers.Utils;
using AktBob.Shared.Contracts;
using System.Text;

namespace AktBob.JobHandlers.Handlers.AddOrUpdateDeskproTicketToGetOrganized;
internal class AddOrUpdateDeskproTicketToGetOrganizedJobHandler(ILogger<AddOrUpdateDeskproTicketToGetOrganizedJobHandler> logger, IServiceScopeFactory serviceScopeFactory) : IJobHandler<AddOrUpdateDeskproTicketToGetOrganizedJob>
{
    private readonly ILogger<AddOrUpdateDeskproTicketToGetOrganizedJobHandler> _logger = logger;
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;

    public async Task Handle(AddOrUpdateDeskproTicketToGetOrganizedJob job, CancellationToken cancellationToken = default)
    {
        var scope = _serviceScopeFactory.CreateScope();
        var queryDispatcher = scope.ServiceProvider.GetRequiredService<IQueryDispatcher>();
        var commandDispatcher = scope.ServiceProvider.GetRequiredService<ICommandDispatcher>();
        var pendingsTickets = scope.ServiceProvider.GetRequiredService<PendingsTickets>();
        var deskproHelper = scope.ServiceProvider.GetRequiredService<DeskproHelper>();

        var currentPendingTicket = new PendingTicket(job.TicketId, job.SubmittedAt);
        pendingsTickets.AddPendingTicket(currentPendingTicket);

        // Check if this submission is the most recent for the specified ticket
        if (!IsMostRecentSubmission(currentPendingTicket, pendingsTickets))
        {
            return;
        }

        List<byte[]> content = new();


        // Get Deskpro Ticket
        var ticketResult = await deskproHelper.GetTicket(queryDispatcher, job.TicketId);

        if (!ticketResult.IsSuccess || ticketResult.Value is null)
        {
            _logger.LogError("Error getting ticket {id} from Deskpro", job.TicketId);
            return;
        }

        var ticket = ticketResult.Value;
        

        // Get custom fields specification
        var ticketCustomFieldsQuery = new GetDeskproCustomFieldSpecificationsQuery();
        var ticketCustomFieldsResult = await queryDispatcher.Dispatch(ticketCustomFieldsQuery, cancellationToken); // TODO: Cache

        if (!ticketCustomFieldsResult.IsSuccess)
        {
            _logger.LogError("Error getting custom fields specifications from Deskpro");
            return;
        }


        // Get ticket agent
        var agent = new PersonDto();
        if (ticket.Agent?.Id is not null)
        {
            var agentResult = await deskproHelper.GetPerson(queryDispatcher, ticket.Agent.Id);
            if (!agentResult.IsSuccess)
            {
                _logger.LogWarning("Error getting person {id} from Deskpro", ticket.Agent.Id);
            }
            else
            {
                agent = agentResult.Value;
            }
        }
        else
        {
            _logger.LogWarning("Deskpro ticket {id} has no assigned agents", job.TicketId);
        }


        // Get ticket user
        var user = new PersonDto();
        if (ticket.Person?.Id is not null)
        {
            var userResult = await deskproHelper.GetPerson(queryDispatcher, ticket.Person.Id);
            if (!userResult.IsSuccess)
            {
                _logger.LogWarning("Error getting person {id} from Deskpro", ticket.Person.Id);
            }
            else
            {
                user = userResult.Value;
            }
        }
        else
        {
            _logger.LogWarning("Deskpro ticket {id} has no assigned agents", job.TicketId);
        }
        


        // Map ticket fields
        var customFields = GenerateCustomFieldValues(job.CustomFieldIds, ticketCustomFieldsResult.Value, ticket);
        var caseNumbers = HtmlHelper.GenerateListOfFieldValues(job.CaseNumberFieldIds, ticket, "ticket-case-numbers.html");

        var ticketDictionary = new Dictionary<string, string>
        {
            { "ticketId", ticket.Id.ToString() },
            { "caseTitle", ticket.Subject },
            { "userName", user.FullName },
            { "userEmail", user.Email },
            { "userPhone", string.Join(", ", user.PhoneNumbers) },
            { "agentName", agent.FullName },
            { "agentEmail", agent.Email },
            { "custom-fields", string.Join("", customFields) },
            { "caseNumbers", string.Join("", caseNumbers) }
        };

        var ticketHtml = HtmlHelper.GenerateHtml("ticket.html", ticketDictionary);
        content.Add(Encoding.UTF8.GetBytes(ticketHtml));


        // Messages
        var getMessagesQuery = new GetDeskproMessagesQuery(ticket.Id);
        var getMessagesResult = await queryDispatcher.Dispatch(getMessagesQuery, cancellationToken);

        if (getMessagesResult.IsSuccess)
        {
            var messages = getMessagesResult.Value.OrderByDescending(x => x.CreatedAt);
            foreach (var message in messages)
            {
                var person = await deskproHelper.GetPerson(queryDispatcher, message.Person?.Id);
                message.Person = person.Value;

                var attachments = Enumerable.Empty<AttachmentDto>();
                if (message.AttachmentIds.Any())
                {
                    attachments = await deskproHelper.GetMessageAttachments(queryDispatcher, ticket.Id, message.Id);
                }

                // Get message number from API database
                var messageNumber = 0;
                var getDatabaseMessageQuery = new GetMessageByDeskproMessageIdQuery(message.Id);
                var getDatabaseMessageResult = await queryDispatcher.Dispatch(getDatabaseMessageQuery, cancellationToken);

                if (!getDatabaseMessageResult.IsSuccess)
                {
                    _logger.LogWarning("No message found in database for Deskpro message ID {id}", message.Id);
                }
                else
                {
                    messageNumber = getDatabaseMessageResult.Value.MessageNumber ?? 0;
                }

                var messageHtml = HtmlHelper.GenerateMessageHtml(message.CreatedAt, message.Person.FullName, message.Person.Email, message.Content, job.GOCaseNumber, ticket.Subject, messageNumber, attachments);
                content.Add(Encoding.UTF8.GetBytes(messageHtml));
            }
        }



        // Generate PDF
        var convertCommand = new ConvertHtmlToPdfCommand(content);
        var convertResult = await commandDispatcher.Dispatch(convertCommand, cancellationToken);

        if (!convertResult.IsSuccess)
        {
            _logger.LogError("Error creating CloudConvert job generating PDF for Deskpro ticket {id}", job.TicketId);
            return;
        }

        var getJobQuery = new GetJobQuery(convertResult.Value.JobId);
        var getJobResult = await queryDispatcher.Dispatch(getJobQuery, cancellationToken);

        if (!getJobResult.IsSuccess)
        {
            _logger.LogError("Error querying job '{id}' from PDF from CloudConvert", convertResult.Value.JobId);
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

        var uploadDocumentCommand = new UploadDocumentCommand(getJobResult.Value, job.GOCaseNumber, fileName, metadata, true);
        var uploadDocumentResult = await commandDispatcher.Dispatch(uploadDocumentCommand, cancellationToken);

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
