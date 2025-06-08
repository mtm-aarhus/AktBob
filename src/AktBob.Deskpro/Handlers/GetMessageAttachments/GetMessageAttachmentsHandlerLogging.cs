using AktBob.Deskpro.Contracts.DTOs;
using AktBob.Shared.Extensions;
using AktBob.Shared.Types.Deskpro;

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

    public async Task<ErrorOr<IReadOnlyCollection<AttachmentDto>>> Handle(MessageId messageId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting Deskpro message {messageId} attachments", messageId);

        var result = await _inner.Handle(messageId, cancellationToken);

        result.Switch(
            _ => _logger.LogInformation("Deskpro message {messageId} attachments retrieved", messageId),
            errors => _logger.LogWarning("{name}: {errors}", nameof(GetMessageAttachmentsHandler), errors.ToCommaDelimitedString()));

        return result;
    }
}
