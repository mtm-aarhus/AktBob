using AktBob.Shared.Types.Deskpro;

namespace Aktbob.Modules.Deskpro.Features.GetMessageAttachments;
internal class GetMessageAttachmentsHandlerException(
    IGetMessageAttachmentsHandler inner,
    ILogger<GetMessageAttachmentsHandler> logger) : IGetMessageAttachmentsHandler
{
    private readonly IGetMessageAttachmentsHandler _inner = inner;
    private readonly ILogger<GetMessageAttachmentsHandler> _logger = logger;

    public async Task<ErrorOr<IReadOnlyCollection<AttachmentDto>>> Handle(MessageId messageId, CancellationToken cancellationToken)
    {
        try
        {
            return await _inner.Handle(messageId, cancellationToken);
        }
        catch (HttpRequestException ex)
        when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return Error.NotFound("GetMessageAttachmentsHandler.NotFound", $"Message {messageId} attachments not found.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {name}", nameof(GetMessageAttachmentsHandler));
            return Error.Failure("GetMessageAttachmentsHandler.Failure", ex.Message);
        }
    }
}
