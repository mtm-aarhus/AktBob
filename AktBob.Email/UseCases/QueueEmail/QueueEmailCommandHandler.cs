using AktBob.Email.Contracts;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace AktBob.Email.UseCases.QueueEmail;
internal class QueueEmailCommandHandler : IRequestHandler<QueueEmailCommand>
{
    private readonly IQueueService _queueService;
    private readonly ILogger<QueueEmailCommandHandler> _logger;
    private readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

    public QueueEmailCommandHandler(IQueueService queueService, ILogger<QueueEmailCommandHandler> logger)
    {
        _queueService = queueService;
        _logger = logger;
    }

    public async Task Handle(QueueEmailCommand request, CancellationToken cancellationToken)
    {
        var dto = new EmailMessageDto(request.To, request.Subject, request.Body);
        var message = JsonSerializer.Serialize(dto, _jsonSerializerOptions);

        var messageId = await _queueService.Queue.QueueMessage(message);

        _logger.LogInformation("Email message queued (queue id {messageId})", messageId);
    }
}
