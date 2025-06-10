using AktBob.Shared.Contracts.Modules.Deskpro.DTOs;
using AktBob.Shared.Extensions;

namespace Aktbob.Modules.Deskpro.Features.GetMessageAttachments;
internal class GetMessageAttachmentsHandlerLogging(
    IGetMessageAttachmentsHandler inner,
    ILogger<GetMessageAttachmentsHandler> logger) : IGetMessageAttachmentsHandler
{
    private readonly IGetMessageAttachmentsHandler _inner = inner;
    private readonly ILogger<GetMessageAttachmentsHandler> _logger = logger;

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
