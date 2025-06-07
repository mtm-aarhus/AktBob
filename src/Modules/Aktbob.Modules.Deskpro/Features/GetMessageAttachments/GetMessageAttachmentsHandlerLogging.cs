using Aktbob.Modules.Deskpro.Contracts.DTOs;
using AktBob.Shared.Extensions;
using AktBob.Shared.Types.Deskpro;

namespace Aktbob.Modules.Deskpro.Features.GetMessageAttachments;
internal class GetMessageAttachmentsHandlerLogging(
    IGetMessageAttachmentsHandler inner,
    ILogger<GetMessageAttachmentsHandler> logger) : IGetMessageAttachmentsHandler
{
    private readonly IGetMessageAttachmentsHandler _inner = inner;
    private readonly ILogger<GetMessageAttachmentsHandler> _logger = logger;

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
