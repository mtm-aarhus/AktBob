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

namespace AktBob.JournalizeDocuments.BackgroundServices;
internal class JournalizeFullTicketDocumentBackgroundService : BackgroundService
{
    private readonly IMediator _mediator;
    private readonly IConfiguration _configuration;
    private readonly ILogger<JournalizeFullTicketDocumentBackgroundService> _logger;
    private readonly IGetOrganizedClient _getOrganizedClient;
    private readonly IHttpClientFactory _httpClientFactory;

    public JournalizeFullTicketDocumentBackgroundService(IMediator mediator, IConfiguration configuration, ILogger<JournalizeFullTicketDocumentBackgroundService> logger, IGetOrganizedClient getOrganizedClient, IHttpClientFactory httpClientFactory)
    {
        _mediator = mediator;
        _configuration = configuration;
        _logger = logger;
        _getOrganizedClient = getOrganizedClient;
        _httpClientFactory = httpClientFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var delaySeconds = _configuration.GetValue<int?>("JournalizeFullDeskproTicket:WorkerIntervalSeconds") ?? 300;
        var journalizeAfterUpload = _configuration.GetValue<bool?>("JournalizeDeskproMessages:JournalizeAfterUpload") ?? false;

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromSeconds(delaySeconds), stoppingToken);

            try
            {
                if (!GetMessagesNotJournalized(out var messages))
                {
                    continue;
                }

                foreach (var message in messages.OrderBy(m => m.DeskproMessageId))
                {
                    if (string.IsNullOrEmpty(message.GOCaseNumber))
                    {
                        continue;
                    }

                    if (message.GODocumentId is not null)
                    {
                        continue;
                    }

                    _logger.LogInformation("Deskpro message ready for upload to GetOrganized. DeskproTicketId {deskproTicketId}, DeskproMessageId {deskproMessageId}", message.DeskproTicketId, message.DeskproMessageId);


                    // Get Deskpro ticket
                    if (!GetDeskproTicket(message, out TicketDto? deskproTicket))
                    {
                        continue;
                    }


                    // Get Deskpro message
                    if (!GetDeskproMessage(message, out MessageDto? deskproMessage))
                    {
                        continue;
                    }


                    // Get Deskpro person
                    PersonDto? person = await GetDeskproPerson(deskproMessage!);


                    // Get attachments
                    GetDeskproMessageAttachments(message.DeskproTicketId, message.DeskproMessageId, deskproMessage?.AttachmentIds.Any() ?? false, out IEnumerable<AttachmentDto> attachments);


                    // Generate PDF document
                    var generateDocumentResult = await GenerateDocument(message, deskproTicket!, deskproMessage!, person, attachments);
                    if (!generateDocumentResult.IsSuccess)
                    {
                        continue;
                    }

                    // Get message creation time and convert to Danish time zone
                    DateTime createdAtUtc = DateTime.SpecifyKind(deskproMessage!.CreatedAt, DateTimeKind.Utc);
                    TimeZoneInfo tzi = TimeZoneInfo.FindSystemTimeZoneById("Romance Standard Time");
                    DateTime createdAtDanishTime = TimeZoneInfo.ConvertTimeFromUtc(createdAtUtc, tzi);

                    var metadata = new UploadDocumentMetadata
                    {
                        DocumentDate = createdAtDanishTime,
                        DocumentCategory = deskproMessage.IsAgentNote ? DocumentCategory.Intern : MapDocumentCategoryFromPerson(person)
                    };

                    var fileName = GenerateFileName(message, person, createdAtDanishTime);


                    // Upload parent document
                    if (!UploadDocumentToGO(generateDocumentResult.Value, message.GOCaseNumber, "Dokumenter", string.Empty, fileName, metadata, out int? parentDocumentId, stoppingToken))
                    {
                        continue;
                    }

                    // Update database
                    var updateMessageCommand = new UpdateMessageSetGoDocumentIdCommand(message.Id, (int)parentDocumentId!);
                    await _mediator.Send(updateMessageCommand);


                    // Handle message attachments
                    await ProcessAttachments(attachments, message.GOCaseNumber, metadata, parentDocumentId, stoppingToken);


                    // Finalize the parent document
                    // IMPORTANT: the parent document must not be finalized before the attachments has been set as children
                    await _getOrganizedClient.FinalizeDocument((int)parentDocumentId, false, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }
    }


    private async Task ProcessAttachments(IEnumerable<AttachmentDto> attachments, string caseNumber, UploadDocumentMetadata metadata, int? parentDocumentId, CancellationToken stoppingToken)
    {
        if (!attachments.Any())
        {
            return;
        }

        var childrenDocumentIds = new List<int>();

        foreach (var attachment in attachments)
        {
            // Get the individual attachments as a stream
            using (var stream = new MemoryStream())
            {
                var getAttachmentStreamQuery = new GetDeskproMessageAttachmentQuery(attachment.DownloadUrl);
                var getAttachmentStreamResult = await _mediator.Send(getAttachmentStreamQuery, stoppingToken);

                if (!getAttachmentStreamResult.IsSuccess)
                {
                    _logger.LogError("Error downloading attachment '{filename}' from Deskpro message #{messageId}, ticketId {ticketId}", attachment.FileName, attachment.MessageId, attachment.TicketId);
                    continue;
                }

                getAttachmentStreamResult.Value.CopyTo(stream);
                var attachmentBytes = stream.ToArray();

                // Upload the attachment to GO
                if (!UploadDocumentToGO(attachmentBytes, caseNumber, "Dokumenter", string.Empty, attachment.FileName, metadata, out int? attachmentDocumentId, stoppingToken))
                {
                    continue;
                }

                // Finalize the attachment
                await _getOrganizedClient.FinalizeDocument((int)attachmentDocumentId!, false, stoppingToken);
                childrenDocumentIds.Add((int)attachmentDocumentId!);
            }
        }

        // Set attachments as children
        if (childrenDocumentIds.Count > 0)
        {
            await _getOrganizedClient.RelateDocuments((int)parentDocumentId!, childrenDocumentIds.ToArray());
        }
    }


    private static string GenerateFileName(DatabaseAPI.Contracts.DTOs.MessageDto message, PersonDto? person, DateTime createdAtDanishTime)
    {
        // Using a list of strings to construct the title so we later can join them with a space separator.
        // Just a lazy way for not worry about space seperators manually...
        var titleElements = new List<string>
        {
            "Besked"
        };

        if (message.MessageNumber.HasValue)
        {
            titleElements.Add($"({message.MessageNumber?.ToString("D3")})");
        }

        if (person is not null)
        {
            titleElements.Add(person.FullName);
        }

        titleElements.Add($"({createdAtDanishTime.ToString("dd-MM-yyyy HH.mm.ss")}).pdf");
        var title = string.Join(" ", titleElements);

        return title;
    }


    private bool UploadDocumentToGO(byte[] bytes, string caseNumber, string listName, string folderPath, string fileName, UploadDocumentMetadata metadata, out int? documentId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Uploading document to GetOrganized (CaseNumber: {caseNumber}, FileName: '{filename}', file size (bytes): {filesize}) ...", caseNumber, fileName, bytes.Length);

        documentId = null;

        var uploadResult = _getOrganizedClient.UploadDocument(
                            bytes,
                            caseNumber,
                            listName,
                            folderPath,
                            fileName,
                            metadata,
                            cancellationToken).GetAwaiter().GetResult();

        if (uploadResult is not null)
        {
            documentId = uploadResult.DocumentId;
            return true;
        }

        _logger.LogError("Error uploading document to GetOrganized (CaseNumber: {caseNumber}, FileName: '{filename}', file size (bytes): {filesize})", caseNumber, fileName, bytes.Length);
        return false;
    }


    private bool GetMessagesNotJournalized(out IEnumerable<DatabaseAPI.Contracts.DTOs.MessageDto> messages)
    {
        messages = Enumerable.Empty<DatabaseAPI.Contracts.DTOs.MessageDto>();

        var getMessagesQuery = new GetMessagesNotJournalizedQuery();
        var getMessagesQueryResult = _mediator.Send(getMessagesQuery).GetAwaiter().GetResult();

        if (getMessagesQueryResult.Status == ResultStatus.NotFound)
        {
            return false;
        }

        if (!getMessagesQueryResult.IsSuccess)
        {
            _logger.LogError("Error requesting database API for messages not journalized");
            return false;
        }

        messages = getMessagesQueryResult.Value;
        return true;
    }


    private DocumentCategory MapDocumentCategoryFromPerson(PersonDto? person)
    {
        if (person is null)
        {
            return DocumentCategory.Intern;
        }

        return person.IsAgent ? DocumentCategory.Udgående : DocumentCategory.Indgående;
    }


    private async Task<Result<byte[]>> GenerateDocument(DatabaseAPI.Contracts.DTOs.MessageDto databaseMessageDto, TicketDto deskproTicket, MessageDto deskproMessageDto, PersonDto? person, IEnumerable<AttachmentDto> attachmentDtos, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Generating PDF document from Deskpro message #{id}", deskproMessageDto.Id);

        var attachmentFileNames = attachmentDtos.Select(a => a.FileName) ?? Enumerable.Empty<string>();
        var dto = new JournalizeMessageDto
        {
            Subject = deskproTicket.Subject,
            MessageId = deskproMessageDto.Id,
            MessageNumber = databaseMessageDto.MessageNumber ?? 0,
            CreatedAt = deskproMessageDto.CreatedAt,
            MessageContent = deskproMessageDto.Content,
            PersonFromEmail = deskproMessageDto.Person.Email,
            PersonFromName = deskproMessageDto.Person.FullName,
            AttachmentFileNames = attachmentDtos.Select(a => a.FileName).ToArray(),
        };

        try
        {
            var httpClient = _httpClientFactory.CreateClient(Constants.DESKPRO_PDF_GENERATOR_HTTP_CLIENT_NAME);
            var json = JsonSerializer.Serialize(dto, new JsonSerializerOptions { Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping });
            var stringContent = new StringContent(json, Encoding.UTF8, "application/json");
            var requestMessage = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri("/Generate/SingleMessage", UriKind.Relative),
                Content = stringContent
            };

            var response = await httpClient.SendAsync(requestMessage, cancellationToken);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsByteArrayAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError("Error generating PDF document for Deskpro message #{messageId}. Error message: {message}", databaseMessageDto.DeskproMessageId, ex.Message);
            return Result.Error();
        }
    }


    private async Task<PersonDto?> GetDeskproPerson(MessageDto deskproMessage)
    {
        _logger.LogInformation("Getting Deskpro person #{id}", deskproMessage!.Person.Id);

        var getDeskproPersonQuery = new GetDeskproPersonQuery(deskproMessage.Person.Id);
        var getDeskproPersonQueryResult = await _mediator.Send(getDeskproPersonQuery);

        if (!getDeskproPersonQueryResult.IsSuccess)
        {
            _logger.LogWarning("Could not get Deskpro getting person #{id}", deskproMessage.Person.Id);
        }

        if (string.IsNullOrEmpty(getDeskproPersonQueryResult.Value?.Email))
        {
            _logger.LogWarning("No email for Deskpro person #{id}", deskproMessage.Person.Id);
        }

        return getDeskproPersonQueryResult.Value;
    }


    private bool GetDeskproMessage(DatabaseAPI.Contracts.DTOs.MessageDto? message, out MessageDto? messageDto)
    {
        messageDto = null;

        if (message == null)
        {
            _logger.LogError("Queue message does not contain a valid Deskpro Message Id");
            return false;
        }

        _logger.LogInformation("Getting Deskpro message #{id}", message.DeskproMessageId);

        var getDeskproMessageQuery = new GetDeskproMessageByIdQuery(message.DeskproTicketId, message.DeskproMessageId);
        var getDeskproMessageQueryResult = _mediator.Send(getDeskproMessageQuery).GetAwaiter().GetResult();

        if (!getDeskproMessageQueryResult.IsSuccess)
        {
            _logger.LogError("Error requesting Deskpro message #{id}. Message will be marked as 'deleted' in database.", message.DeskproMessageId);

            var deleteMessageCommand = new DeleteMessageCommand(message.Id);
            _mediator.Send(deleteMessageCommand).GetAwaiter().GetResult();

            return false;
        }

        messageDto = getDeskproMessageQueryResult.Value;
        return true;
    }


    private bool GetDeskproMessageAttachments(int deskproTicketId, int deskproMessageId, bool messageHasAttachments, out IEnumerable<AttachmentDto> attachmentDtos)
    {
        attachmentDtos = Enumerable.Empty<AttachmentDto>();

        if (!messageHasAttachments)
        {
            return false;
        }

        _logger.LogInformation("Getting Deskpro message #{id} attachments", deskproMessageId);

        var getDeskproMessageAttachmentsQuery = new GetDeskproMessageAttachmentsQuery(deskproTicketId, deskproMessageId);
        var getDeskproMessageAttachmentsResult = _mediator.Send(getDeskproMessageAttachmentsQuery).GetAwaiter().GetResult();

        if (!getDeskproMessageAttachmentsResult.IsSuccess)
        {
            _logger.LogError("Error getting attachments for Deskpro message #{id}.", deskproMessageId);
            return false;
        }

        attachmentDtos = getDeskproMessageAttachmentsResult.Value;
        return true;
    }


    private bool GetDeskproTicket(DatabaseAPI.Contracts.DTOs.MessageDto? message, out TicketDto? ticketDto)
    {
        ticketDto = null;

        if (message == null)
        {
            _logger.LogError("Queue message does not contain a valid Deskpro Ticket Id");
            return false;
        }

        _logger.LogInformation("Getting Deskpro ticket #{id}", message.DeskproTicketId);

        var getDeskproTicketQuery = new GetDeskproTicketByIdQuery(message.DeskproTicketId);
        var getDeskproTicketQueryResult = _mediator.Send(getDeskproTicketQuery).GetAwaiter().GetResult();

        if (!getDeskproTicketQueryResult.IsSuccess)
        {
            _logger.LogError("Error requesting Deskpro ticket #{id}", message.DeskproTicketId);
            return false;
        }

        ticketDto = getDeskproTicketQueryResult.Value;
        return true;
    }
}