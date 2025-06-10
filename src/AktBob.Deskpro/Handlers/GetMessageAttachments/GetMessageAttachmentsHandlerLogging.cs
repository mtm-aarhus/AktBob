using AktBob.Deskpro.Contracts.DTOs;
using AktBob.Shared.Extensions;

namespace AktBob.Deskpro.Handlers.GetMessageAttachments;
internal class GetMessageAttachmentsHandlerLogging : IGetMessageAttachmentsHandler
{
    private readonly IGetMessageAttachmentsHandler _inner;
    private readonly ILogger<GetMessageAttachmentsHandler> _logger;

    public GetMessageAttachmentsHandlerLogging(IGetMessageAttachmentsHandler inner, ILogger<GetMessageAttachmentsHandler> logger)
    {
        _inner = inner;
        _logger = logger;
    }

    public async Task<ErrorOr<IReadOnlyCollection<AttachmentDto>>> Handle(int ticketId, int messageId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting Deskpro ticket {ticketId} message {messageId} attachments", ticketId, messageId);

        var result = await _inner.Handle(ticketId, messageId, cancellationToken);

        result.Switch(
            _ => _logger.LogInformation("Deskpro message {messageId} attachments retrieved", messageId),
            errors => _logger.LogWarning("{name}: {errors}", nameof(GetMessageAttachmentsHandler), errors.ToCommaDelimitedString()));

        return result;
    }
}
