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
            await Task.Delay(TimeSpan.FromSeconds(delaySeconds), stoppingToken);

            var getMessagesQuery = new GetMessagesNotJournalizedQuery();
            var getMessagesQueryResult = await _mediator.Send(getMessagesQuery);

            if (getMessagesQueryResult.Status == Ardalis.Result.ResultStatus.NotFound)
            {
                continue;
            }

            if (!getMessagesQueryResult.IsSuccess)
            {
                continue;
            }

            foreach (var message in getMessagesQueryResult.Value.OrderBy(m => m.DeskproMessageId))
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

                var getDeskproTicketQuery = new GetDeskproTicketByIdQuery(message.DeskproTicketId);
                var getDeskproTicketQueryResult = await _mediator.Send(getDeskproTicketQuery);

                if (!getDeskproTicketQueryResult.IsSuccess)
                {
                    _logger.LogError("Error requesting Deskpro ticket ID {id}", message.DeskproTicketId);
                    continue;
                }

                var deskproTicket = getDeskproTicketQueryResult.Value;

                _logger.LogInformation("Getting message {id} from Deskpro ...", message.DeskproMessageId);

                var getDeskproMessageQuery = new GetDeskproMessageByIdQuery(message.DeskproTicketId, message.DeskproMessageId);
                var getDeskproMessageQueryResult = await _mediator.Send(getDeskproMessageQuery);

                if (!getDeskproMessageQueryResult.IsSuccess)
                {
                    _logger.LogError("Error requesting Deskpro message ID {id}", message.DeskproMessageId);
                    continue;
                }

                var deskproMessage = getDeskproMessageQueryResult.Value;

                _logger.LogInformation("Getting person {id} from Deskpro ...", deskproMessage.Person.Id);

                var getDeskproPersonQuery = new GetDeskproPersonQuery(deskproMessage.Person.Id);
                var getDeskproPersonQueryResult = await _mediator.Send(getDeskproPersonQuery);

                if (!getDeskproPersonQueryResult.IsSuccess)
                {
                    _logger.LogWarning("Error getting person {id} from Deskpro", deskproMessage.Person.Id);
                }

                if (string.IsNullOrEmpty(getDeskproPersonQueryResult.Value.Email))
                {
                    _logger.LogWarning("No email for person {id} in the response from Deskpro", deskproMessage.Person.Id);
                }

                var person = getDeskproPersonQueryResult.Value;

                _logger.LogInformation("Generating PDF document from message data ...");

                var generateDocumentCommand = new GenerateDeskproMessageDocumentCommand(
                    TicketSubject: deskproTicket?.Subject ?? string.Empty,
                    MessageId: deskproMessage.Id,
                    MessageContent: deskproMessage.Content,
                    CreatedAt: deskproMessage.CreatedAt,
                    PersonName: person?.FullName ?? string.Empty,
                    PersonEmail: person?.Email ?? string.Empty);

                var generatorDocumentCommandResult = await _mediator.Send(generateDocumentCommand);

                if (!generatorDocumentCommandResult.IsSuccess)
                {
                    _logger.LogError("Error generating PDF document for Deskpro message {messageId}", message.DeskproMessageId);
                    continue;
                }

                try
                {
                    // Get message creation time and convert to Danish time zone
                    DateTime createdAtUtc = DateTime.SpecifyKind(deskproMessage.CreatedAt, DateTimeKind.Utc);
                    TimeZoneInfo tzi = TimeZoneInfo.FindSystemTimeZoneById("Romance Standard Time");
                    DateTime createdAtDanishTime = TimeZoneInfo.ConvertTimeFromUtc(createdAtUtc, tzi);

                    // Specify GO document metadata
                    var metadata = new UploadDocumentMetadata
                    {
                        DocumentDate = createdAtDanishTime
                    };

                    var title = $"Korrespondance ({deskproMessage.Id}) {(person is not null ? $"fra {person.FullName}" : string.Empty)} ({createdAtDanishTime.ToString("dd-MM-yyyy HH.mm.ss")}).pdf";

                    _logger.LogInformation("Uploading document to GetOrganized (CaseNumber: {caseNumber}, Document title: '{title}', Document date: '{date}', file size (bytes): {filesize}) ...", message.GOCaseNumber, title, createdAtDanishTime.ToString("dd-MM-yyyy HH.mm.ss"), generatorDocumentCommandResult.Value.Length);

                    // Upload to GO
                    var uploadResult = await _getOrganizedClient.UploadDocument(
                        bytes: generatorDocumentCommandResult.Value,
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
                        _logger.LogError("Error uploading document to GetOrganized (CaseNumber: {caseNumber}, Document title: '{title}', Document date: '{date}', file size (bytes): {filesize})", message.GOCaseNumber, title, createdAtDanishTime.ToString("dd-MM-yyyy HH.mm.ss"), generatorDocumentCommandResult.Value.Length);
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
    }
}
