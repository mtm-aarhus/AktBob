using AAK.GetOrganized;
using AAK.GetOrganized.UploadDocument;
using AktBob.CloudConvert.Contracts;
using AktBob.Database.Contracts;
using AktBob.Deskpro.Contracts;
using AktBob.Deskpro.Contracts.DTOs;
using AktBob.GetOrganized.Contracts;
using AktBob.JobHandlers.Utils;
using AktBob.Shared;
using AktBob.Shared.Contracts;
using MassTransit;
using MassTransit.Mediator;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text;

namespace AktBob.JobHandlers.Handlers.AddOrUpdateDeskproTicketToGetOrganized;
internal class AddOrUpdateDeskproTicketToGetOrganizedJobHandler(
    ILogger<AddOrUpdateDeskproTicketToGetOrganizedJobHandler> logger,
    DeskproHelper deskproHelper,
    IServiceScopeFactory serviceScopeFactory,
    PendingsTickets pendingsTickets) : IJobHandler<AddOrUpdateDeskproTicketToGetOrganizedJob>
{
    private readonly ILogger<AddOrUpdateDeskproTicketToGetOrganizedJobHandler> _logger = logger;
    private readonly DeskproHelper _deskproHelper = deskproHelper;
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;
    private readonly PendingsTickets _pendingsTickets = pendingsTickets;

    public async Task Handle(AddOrUpdateDeskproTicketToGetOrganizedJob job, CancellationToken cancellationToken = default)
    {
        var scope = _serviceScopeFactory.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
 
        var currentPendingTicket = new PendingTicket(job.TicketId, job.SubmittedAt);
        _pendingsTickets.AddPendingTicket(currentPendingTicket);

        // Check if this submission is the most recent for the specified ticket
        if (!IsMostRecentSubmission(currentPendingTicket))
        {
            return;
        }

        List<byte[]> content = new();

        // Get Deskpro Ticket
        var ticketResult = await _deskproHelper.GetDeskproTicket(mediator, job.TicketId);

        if (!ticketResult.IsSuccess)
        {
            // TODO;
            return;
        }

        var ticket = ticketResult.Value;
        if (ticket is null)
        {
            // TODO
            return;
        }

        // Get custom fields specification
        var ticketCustomFieldsQuery = new GetDeskproCustomFieldSpecificationsQuery();
        var ticketCustomFieldsResult = await mediator.SendRequest(ticketCustomFieldsQuery, cancellationToken); // TODO: Cache

        if (!ticketCustomFieldsResult.IsSuccess)
        {
            // TODO
            return;
        }


        // Get ticket agent
        var agentResult = await _deskproHelper.GetDeskproPerson(mediator, ticket.Agent?.Id);
        if (!agentResult.IsSuccess)
        {
            // TODO
        }

        // Get ticket user
        var userResult = await _deskproHelper.GetDeskproPerson(mediator, ticket.Person?.Id);
        if (!userResult.IsSuccess)
        {
            // TODO
        }


        // Map ticket fields
        var customFields = GenerateCustomFieldValues(job.CustomFieldIds, ticketCustomFieldsResult.Value, ticket);
        var caseNumbers = HtmlHelper.GenerateListOfFieldValues(job.CaseNumberFieldIds, ticket, "ticket-case-numbers.html");

        var ticketDictionary = new Dictionary<string, string>
            {
                { "ticketId", ticket.Id.ToString() },
                { "caseTitle", ticket.Subject },
                { "userName", userResult.Value.FullName },
                { "userEmail", userResult.Value.Email },
                { "userPhone", string.Join(", ", userResult.Value.PhoneNumbers) },
                { "agentName", agentResult.Value.FullName },
                { "agentEmail", agentResult.Value.Email },
                { "custom-fields", string.Join("", customFields) },
                { "caseNumbers", string.Join("", caseNumbers) }
            };

        var ticketHtml = HtmlHelper.GenerateHtml("ticket.html", ticketDictionary);
        content.Add(Encoding.UTF8.GetBytes(ticketHtml));


        // Messages
        var getMessagesQuery = new GetDeskproMessagesQuery(ticket.Id);
        var getMessagesResult = await mediator.SendRequest(getMessagesQuery, cancellationToken);

        if (getMessagesResult.IsSuccess)
        {
            foreach (var message in getMessagesResult.Value)
            {
                var person = await _deskproHelper.GetDeskproPerson(mediator, message.Person?.Id);
                message.Person = person.Value;

                var attachments = Enumerable.Empty<AttachmentDto>();
                if (message.AttachmentIds.Any())
                {
                    attachments = await _deskproHelper.GetDeskproMessageAttachments(mediator, ticket.Id, message.Id);
                }

                // Get message number from API database
                var messageNumber = 0;
                var getMessageFromApiDatabaseQuery = new GetMessageByDeskproMessageIdQuery(message.Id);
                var getMessageFromApiDatabaseResult = await mediator.SendRequest(getMessageFromApiDatabaseQuery, cancellationToken);

                if (!getMessageFromApiDatabaseResult.IsSuccess)
                {
                    _logger.LogWarning("No message found in API database for Deskpro message ID {id}", message.Id);
                }
                else
                {
                    messageNumber = getMessageFromApiDatabaseResult.Value.MessageNumber ?? 0;
                }

                var messageHtml = HtmlHelper.GenerateMessageHtml(message, attachments, job.GOCaseNumber, ticket.Subject, messageNumber); // TODO: fix message number
                content.Add(Encoding.UTF8.GetBytes(messageHtml));
            }
        }



        // Generate PDF
        var convertCommand = new ConvertHtmlToPdfCommand(content);
        var convertResult = await mediator.SendRequest(convertCommand, cancellationToken);

        if (!convertResult.IsSuccess)
        {
            // TODO
        }

        var getJobQuery = new GetJobQuery(convertResult.Value.JobId);
        var getJobResult = await mediator.SendRequest(getJobQuery, cancellationToken);

        if (!getJobResult.IsSuccess)
        {
            // TODO
        }


        // Check if this submission is the most recent for the specified ticket
        if (!IsMostRecentSubmission(currentPendingTicket))
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
        var uploadDocumentResult = await mediator.SendRequest(uploadDocumentCommand, cancellationToken);

        if (!uploadDocumentResult.IsSuccess)
        {
            _logger.LogError("Error uploading full ticket document to GetOrganized: Deskpro ticket {ticketId}, GO case '{goCaseNumber}'", job.TicketId, job.GOCaseNumber);
        }
        
        _pendingsTickets.RemovePendingTicket(currentPendingTicket);
    }

    private bool IsMostRecentSubmission(PendingTicket pendingTicket)
    {
        if (!_pendingsTickets.IsMostRecent(pendingTicket))
        {
            _logger.LogInformation("Not the most current submission for updating the Deskpro PDF document. (Deskpro ticket {id}, submittedAt {submittedAt}", pendingTicket.TicketId, pendingTicket.SubmittedAt);
            _pendingsTickets.RemovePendingTicket(pendingTicket);
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
