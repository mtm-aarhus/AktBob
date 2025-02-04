using AktBob.Email.Contracts;
using MassTransit.Mediator;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace AktBob.Email.UseCases.QueueEmail;
public class QueueEmailCommandHandler(IQueueService queueService, ILogger<QueueEmailCommandHandler> logger) : MediatorRequestHandler<QueueEmailCommand>
{
    private readonly IQueueService _queueService = queueService;
    private readonly ILogger<QueueEmailCommandHandler> _logger = logger;

    private readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

    protected override async Task Handle(QueueEmailCommand request, CancellationToken cancellationToken)
    {
        var dto = new EmailMessageDto(request.To, request.Subject, request.Body);
        var message = JsonSerializer.Serialize(dto, _jsonSerializerOptions);

        var messageId = await _queueService.Queue.QueueMessage(message);

        _logger.LogInformation("Email message queued (queue id {messageId})", messageId);
    }
}
