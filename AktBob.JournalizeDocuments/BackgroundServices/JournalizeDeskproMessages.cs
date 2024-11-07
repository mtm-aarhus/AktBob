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

namespace AktBob.JournalizeDocuments.BackgroundServices;
internal class JournalizeDeskproMessages : BackgroundService
{
    private readonly IMediator _mediator;
    private readonly IConfiguration _configuration;
    private readonly ILogger<JournalizeDeskproMessages> _logger;
    private readonly IGetOrganizedClient _getOrganizedClient;

    public JournalizeDeskproMessages(IMediator mediator, IConfiguration configuration, ILogger<JournalizeDeskproMessages> logger, IGetOrganizedClient getOrganizedClient)
    {
        _mediator = mediator;
        _configuration = configuration;
        _logger = logger;
        _getOrganizedClient = getOrganizedClient;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var delaySeconds = _configuration.GetValue<int?>("JournalizeDeskproMessages:WorkerIntervalSeconds") ?? 10;
        var journalizeAfterUpload = _configuration.GetValue<bool?>("JournalizeDeskproMessages:JournalizeAfterUpload") ?? true;

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (GetMessagesNotJournalized(out var messages))
                {
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


                        // Generate PDF document
                        if (!GenerateDocument(message, deskproTicket!, deskproMessage!, person, out byte[]? documentBytes))
                        {
                            continue;
                        }

                        // Get message creation time and convert to Danish time zone
                        DateTime createdAtUtc = DateTime.SpecifyKind(deskproMessage!.CreatedAt, DateTimeKind.Utc);
                        TimeZoneInfo tzi = TimeZoneInfo.FindSystemTimeZoneById("Romance Standard Time");
                        DateTime createdAtDanishTime = TimeZoneInfo.ConvertTimeFromUtc(createdAtUtc, tzi);

                        // Specify GO document metadata
                        var metadata = new UploadDocumentMetadata
                        {
                            DocumentDate = createdAtDanishTime,
                            DocumentCategory = deskproMessage.IsAgentNote ? DocumentCategory.Intern : MapDocumentCategoryFromPerson(person)
                        };

                        var title = $"Besked ({message.MessageNumber.ToString()}) {(person is not null ? $"fra {person.FullName}" : string.Empty)} ({createdAtDanishTime.ToString("dd-MM-yyyy HH.mm.ss")}).pdf";

                        _logger.LogInformation("Uploading document to GetOrganized (CaseNumber: {caseNumber}, Document title: '{title}', Document date: '{date}', file size (bytes): {filesize}) ...", message.GOCaseNumber, title, createdAtDanishTime.ToString("dd-MM-yyyy HH.mm.ss"), documentBytes!.Length);

                        // Upload to GO
                        var uploadResult = await _getOrganizedClient.UploadDocument(
                            bytes: documentBytes,
                            message.GOCaseNumber,
                            "Dokumenter",
                            null,
                            title,
                            metadata,
                            stoppingToken);

                        // Journalize the document
                        if (uploadResult is not null)
                        {
                            // Update database
                            _logger.LogInformation("Updating message in database ...");
                            var updateMessageCommand = new UpdateMessageSetGoDocumentIdCommand(message.Id, uploadResult.DocumentId);
                            await _mediator.Send(updateMessageCommand);

                            if (journalizeAfterUpload)
                            {
                                _logger.LogInformation("Journalizing document {id} ...", uploadResult.DocumentId);
                                await _getOrganizedClient.JournalizeDocument(uploadResult.DocumentId);
                            }
                        }
                        else
                        {
                            _logger.LogError("Error uploading document to GetOrganized (CaseNumber: {caseNumber}, Document title: '{title}', Document date: '{date}', file size (bytes): {filesize})", message.GOCaseNumber, title, createdAtDanishTime.ToString("dd-MM-yyyy HH.mm.ss"), documentBytes.Length);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }

            await Task.Delay(TimeSpan.FromSeconds(delaySeconds), stoppingToken);

        }
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


    private bool GenerateDocument(DatabaseAPI.Contracts.DTOs.MessageDto databaseMessageDto, TicketDto deskproTicket, MessageDto deskproMessageDto, PersonDto? person, out byte[]? bytes)
    {
        bytes = null;

        _logger.LogInformation("Generating PDF document from Deskpro message #{id}", deskproMessageDto.Id);

        var generateDocumentCommand = new GenerateDeskproMessageDocumentCommand(
            TicketSubject: deskproTicket?.Subject ?? string.Empty,
            MessageId: deskproMessageDto.Id,
            MessageNumber: databaseMessageDto.MessageNumber ?? 0,
            MessageContent: deskproMessageDto.Content,
            CreatedAt: deskproMessageDto.CreatedAt,
            PersonName: person?.FullName ?? string.Empty,
            PersonEmail: person?.Email ?? string.Empty);

        var generatorDocumentCommandResult = _mediator.Send(generateDocumentCommand).GetAwaiter().GetResult();

        if (!generatorDocumentCommandResult.IsSuccess)
        {
            _logger.LogError("Error generating PDF document for Deskpro message #{messageId}", databaseMessageDto.DeskproMessageId);
            return false;
        }

        bytes = generatorDocumentCommandResult.Value;
        return true;
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