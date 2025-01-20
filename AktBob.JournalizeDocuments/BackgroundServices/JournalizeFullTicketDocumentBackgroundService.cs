using Microsoft.Extensions.Configuration;
using AAK.GetOrganized.UploadDocument;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MediatR;
using AktBob.DatabaseAPI.Contracts.Queries;
using AktBob.Deskpro.Contracts;
using AktBob.DocumentGenerator.Contracts;
using AAK.GetOrganized;
using AktBob.DatabaseAPI.Contracts.Commands;
using AktBob.Deskpro.Contracts.DTOs;
using Ardalis.Result;
using System.Text.Json;
using System.Text;
using System.Threading.Tasks.Sources;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using AktBob.CloudConvert.Contracts;
using AktBob.Shared;

namespace AktBob.JournalizeDocuments.BackgroundServices;
internal class JournalizeFullTicketDocumentBackgroundService : BackgroundService
{
    private readonly IMediator _mediator;
    private readonly IConfiguration _configuration;
    private readonly ILogger<JournalizeFullTicketDocumentBackgroundService> _logger;
    private readonly IGetOrganizedClient _getOrganizedClient;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly DeskproHelper _deskproHelper;
    private readonly GetOrganizedHelper _getOrganizedHelper;

    public JournalizeFullTicketDocumentBackgroundService(
        IMediator mediator,
        IConfiguration configuration,
        ILogger<JournalizeFullTicketDocumentBackgroundService> logger,
        IGetOrganizedClient getOrganizedClient,
        IHttpClientFactory httpClientFactory,
        DeskproHelper deskproHelper,
        GetOrganizedHelper getOrganizedHelper)
    {
        _mediator = mediator;
        _configuration = configuration;
        _logger = logger;
        _getOrganizedClient = getOrganizedClient;
        _httpClientFactory = httpClientFactory;
        _deskproHelper = deskproHelper;
        _getOrganizedHelper = getOrganizedHelper;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var delaySeconds = _configuration.GetValue<int?>("JournalizeFullDeskproTicket:WorkerIntervalSeconds") ?? 300;
        var journalizeAfterUpload = _configuration.GetValue<bool?>("JournalizeDeskproMessages:JournalizeAfterUpload") ?? false;

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {

                // Get queue items
                var ticketId = 2084;
                int[] customFieldIds = [48, 55, 135];
                int[] caseNumberFields = [61, 62, 63];
                var goCaseNumber = "";

                List<byte[]> content = new();

                // Get Deskpro Ticket
                var ticketResult = await _deskproHelper.GetDeskproTicket(ticketId);

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
                var ticketCustomFieldsResult = await _mediator.Send(ticketCustomFieldsQuery, stoppingToken);

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
                var customFields = GenerateCustomFieldValues(customFieldIds, ticketCustomFieldsResult.Value, ticket);
                var caseNumbers = _deskproHelper.GenerateListOfFieldValues(caseNumberFields, ticket, "ticket-case-numbers.html");

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

                var ticketHtml = _deskproHelper.GenerateHtml("ticket.html", ticketDictionary);
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

                    var messageHtml = _deskproHelper.GenerateMessageHtml(message, attachments, goCaseNumber, ticket.Subject, 0); // TODO: fix message number
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

                var uploadDocumentResult = await _getOrganizedHelper.UploadDocumentToGO(getJobResult.Value, goCaseNumber, "Dokumenter", string.Empty, fileName, metadata, stoppingToken);

                if (!uploadDocumentResult.IsSuccess)
                {
                    _logger.LogError("Error uploading full ticket document to GetOrganized: Deskpro ticket {ticketId}, GO case '{goCaseNumber}'", ticketId, goCaseNumber);
                    continue;
                }
                
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
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

            var html = _deskproHelper.GenerateHtml("custom-field.html", dictionary);
            items.Add(html);
        }

        return items;
    }
}