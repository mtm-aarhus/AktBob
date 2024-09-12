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

        while (!stoppingToken.IsCancellationRequested)
        {
            var getMessagesQuery = new GetMessagesNotJournalizedQuery();
            var getMessagesQueryResult = await _mediator.Send(getMessagesQuery);

            if (!getMessagesQueryResult.IsSuccess)
            {
                _logger.LogError("Something went wrong requesting the database API for messages not journalized");
            }
            else
            {
                foreach (var message in getMessagesQueryResult.Value)
                {
                    if (string.IsNullOrEmpty(message.GOCaseNumber))
                    {
                        continue;
                    }

                    if (message.GOJournalizedAt is not null)
                    {
                        continue;
                    }

                    var getDeskproTicketQuery = new GetDeskproTicketByIdQuery(message.DeskproTicketId);
                    var getDeskproTicketQueryResult = await _mediator.Send(getDeskproTicketQuery);

                    if (!getDeskproTicketQueryResult.IsSuccess)
                    {
                        _logger.LogError("Error requesting Deskpro ticket ID {id}", message.DeskproTicketId);
                        continue;
                    }

                    var deskproTicket = getDeskproTicketQueryResult.Value;

                    var getDeskproMessageQuery = new GetDeskproMessageByIdQuery(message.DeskproTicketId, message.DeskproMessageId);
                    var getDeskproMessageQueryResult = await _mediator.Send(getDeskproMessageQuery);

                    if (!getDeskproMessageQueryResult.IsSuccess)
                    {
                        _logger.LogError("Error requesting Deskpro message ID {id}", message.DeskproMessageId);
                        continue;
                    }

                    var deskproMessage = getDeskproMessageQueryResult.Value;

                    var getDeskproPersonQuery = new GetDeskproPersonQuery(deskproMessage.Person.Id);
                    var getDeskproPersonQueryResult = await _mediator.Send(getDeskproPersonQuery);

                    var person = getDeskproPersonQueryResult.Value;

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
                        _logger.LogError("Error generating PDF document for message (database ID {messageId})", message.Id);
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

                        var titel = $"Korrespondance ({deskproMessage.Id}) {(person is not null ? $"fra {person.FullName}" : string.Empty)} ({createdAtDanishTime.ToString("dd-MM-yyyy HH.mm.ss")}).pdf";

                        // Upload to GO
                        var uploadResult = await _getOrganizedClient.UploadDocument(
                            bytes: generatorDocumentCommandResult.Value,
                            message.GOCaseNumber,
                            "Dokumenter",
                            null,
                            titel,
                            metadata);

                        // Journalize the document
                        if (uploadResult is not null)
                        {
                            await _getOrganizedClient.JournalizeDocument(uploadResult.DocumentId);

                            // Update database
                            var updateMessageCommand = new UpdateMessageSetJournalizedCommand(message.Id, DateTime.UtcNow, uploadResult.DocumentId);
                            await _mediator.Send(updateMessageCommand);
                        }
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }

                await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
            }
        }
    }
}
