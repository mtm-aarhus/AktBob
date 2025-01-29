using Microsoft.Extensions.Configuration;
using AAK.GetOrganized.UploadDocument;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MediatR;
using AktBob.Deskpro.Contracts;
using AAK.GetOrganized;
using AktBob.Deskpro.Contracts.DTOs;
using System.Text.Json;
using System.Text;
using AktBob.CloudConvert.Contracts;
using AktBob.Shared;
using Ardalis.GuardClauses;
using AktBob.Queue.Contracts;

namespace AktBob.JournalizeDocuments.BackgroundServices;
internal class JournalizeFullTicketDocumentBackgroundService : BackgroundService
{
    private readonly IMediator _mediator;
    private readonly IConfiguration _configuration;
    private readonly ILogger<JournalizeFullTicketDocumentBackgroundService> _logger;
    private readonly DeskproHelper _deskproHelper;
    private readonly GetOrganizedHelper _getOrganizedHelper;

    public JournalizeFullTicketDocumentBackgroundService(
        IMediator mediator,
        IConfiguration configuration,
        ILogger<JournalizeFullTicketDocumentBackgroundService> logger,
        DeskproHelper deskproHelper,
        GetOrganizedHelper getOrganizedHelper)
    {
        _mediator = mediator;
        _configuration = configuration;
        _logger = logger;
        _deskproHelper = deskproHelper;
        _getOrganizedHelper = getOrganizedHelper;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var delaySeconds = _configuration.GetValue<int?>("JournalizeFullDeskproTicket:WorkerIntervalSeconds") ?? 300;
        var azureQueueName = Guard.Against.NullOrEmpty(_configuration.GetValue<string>($"JournalizeFullDeskproTicket:AzureQueueName"));

        while (!stoppingToken.IsCancellationRequested)
        {
            var queueItems = await GetQueueItems(azureQueueName, stoppingToken);

            foreach (var queueItem in queueItems)
            {
                var body = queueItem.Body.ToString();
                var bodyDecoded = Encoding.UTF8.GetString(Convert.FromBase64String(body));
                var item = JsonSerializer.Deserialize<JournalizeFullTicketQueueItemDto>(bodyDecoded, new JsonSerializerOptions {  PropertyNameCaseInsensitive = true, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping });

                if (item == null)
                {
                    // TODO
                    continue;
                }

                try
                {
                    List<byte[]> content = new();

                    // Get Deskpro Ticket
                    var ticketResult = await _deskproHelper.GetDeskproTicket(item.TicketId);

                    if (!ticketResult.IsSuccess)
                    {
                        // TODO;
                        continue;
                    }

                    var ticket = ticketResult.Value;
                    if (ticket is null)
                    {
                        // TODO
                        continue;
                    }

                    // Get custom fields specification
                    var ticketCustomFieldsQuery = new GetDeskproCustomFieldSpecificationsQuery();
                    var ticketCustomFieldsResult = await _mediator.Send(ticketCustomFieldsQuery, stoppingToken); // TODO: Cache

                    if (!ticketCustomFieldsResult.IsSuccess)
                    {
                        // TODO
                        continue;
                    }


                    // Get ticket agent
                    var agentResult = await _deskproHelper.GetDeskproPerson(ticket.Agent?.Id);
                    if (!agentResult.IsSuccess)
                    {
                        // TODO
                    }

                    // Get ticket user
                    var userResult = await _deskproHelper.GetDeskproPerson(ticket.Person?.Id);
                    if (!userResult.IsSuccess)
                    {
                        // TODO
                    }


                    // Map ticket fields
                    var customFields = GenerateCustomFieldValues(item.CustomFieldIds, ticketCustomFieldsResult.Value, ticket);
                    var caseNumbers = HtmlHelper.GenerateListOfFieldValues(item.CaseNumberFieldIds, ticket, "ticket-case-numbers.html");

                    var ticketDictionary = new Dictionary<string, string>
                {
                    { "ticketId", ticket.Id.ToString() },
                    { "caseTitle", ticket.Subject },
                    { "userName", userResult.Value.FullName },
                    { "userEmail", userResult.Value.Email },
                    { "userPhone", string.Join(", ", userResult.Value.PhoneNumbers) },
                    { "agentName", agentResult.Value.FullName },
                    { "custom-fields", string.Join("", customFields) },
                    { "caseNumbers", string.Join("", caseNumbers) }
                };

                    var ticketHtml = HtmlHelper.GenerateHtml("ticket.html", ticketDictionary);
                    content.Add(Encoding.UTF8.GetBytes(ticketHtml));


                    // Messages
                    var messages = await _deskproHelper.GetDeskproMessages(ticket.Id);
                    foreach (var message in messages)
                    {
                        var person = await _deskproHelper.GetDeskproPerson(message.Person?.Id);
                        message.Person = person.Value;

                        var attachments = Enumerable.Empty<AttachmentDto>();
                        if (message.AttachmentIds.Any())
                        {
                            attachments = await _deskproHelper.GetDeskproMessageAttachments(ticket.Id, message.Id);
                        }

                        // Get message number from API database
                        var messageNumber = 0;
                        var getMessageFromApiDatabaseQuery = new DatabaseAPI.Contracts.Queries.GetMessageByDeskproMessageIdQuery(message.Id);
                        var getMessageFromApiDatabaseResult = await _mediator.Send(getMessageFromApiDatabaseQuery, stoppingToken);
                        
                        if (!getMessageFromApiDatabaseResult.IsSuccess)
                        {
                            _logger.LogWarning("No message found in API database for Deskpro message ID {id}", message.Id);
                        }
                        else
                        {
                            messageNumber = getMessageFromApiDatabaseResult.Value.MessageNumber ?? 0;
                        }
                        
                        var messageHtml = HtmlHelper.GenerateMessageHtml(message, attachments, item.GOCaseNumber, ticket.Subject, messageNumber); // TODO: fix message number
                        content.Add(Encoding.UTF8.GetBytes(messageHtml));
                    }



                    // Generate PDF
                    var convertCommand = new ConvertHtmlToPdfCommand(content);
                    var convertResult = await _mediator.Send(convertCommand, stoppingToken);

                    if (!convertResult.IsSuccess)
                    {
                        // TODO
                    }

                    var getJobQuery = new GetJobQuery(convertResult.Value.JobId);
                    var getJobResult = await _mediator.Send(getJobQuery, stoppingToken);

                    if (!getJobResult.IsSuccess)
                    {
                        // TODO
                    }


                    // Upload to GO
                    var metadata = new UploadDocumentMetadata
                    {
                        DocumentDate = DateTime.UtcNow.UtcToDanish(),
                        DocumentCategory = DocumentCategory.Intern
                    };

                    var fileName = "Samlet korrespondance.pdf";

                    var uploadDocumentResult = await _getOrganizedHelper.UploadDocumentToGO(
                        bytes: getJobResult.Value,
                        caseNumber: item.GOCaseNumber,
                        fileName: fileName,
                        metadata: metadata, 
                        overwrite: true,
                        cancellationToken: stoppingToken);

                    if (!uploadDocumentResult.IsSuccess)
                    {
                        _logger.LogError("Error uploading full ticket document to GetOrganized: Deskpro ticket {ticketId}, GO case '{goCaseNumber}'", item.TicketId, item.GOCaseNumber);
                        continue;
                    }

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                }
            }

            await Task.Delay(TimeSpan.FromSeconds(delaySeconds), stoppingToken);
        }
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


    private async Task<IEnumerable<QueueMessageDto>> GetQueueItems(string queueName, CancellationToken cancellationToken)
    {
        var getQueueMessagesQuery = new GetQueueMessagesQuery(queueName);
        var getQueueMessagesResult = await _mediator.Send(getQueueMessagesQuery, cancellationToken);

        if (!getQueueMessagesResult.IsSuccess)
        {
            return Enumerable.Empty<QueueMessageDto>();
        }

        return getQueueMessagesResult.Value;
    }
}