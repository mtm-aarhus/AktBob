using AktBob.Deskpro.Contracts.DTOs;

namespace AktBob.Deskpro.Handlers.GetMessageAttachments;
internal class GetMessageAttachmentsHandlerException : IGetMessageAttachmentsHandler
{
    private readonly IGetMessageAttachmentsHandler _inner;
    private readonly ILogger<GetMessageAttachmentsHandler> _logger;

    public GetMessageAttachmentsHandlerException(IGetMessageAttachmentsHandler inner, ILogger<GetMessageAttachmentsHandler> logger)
    {
        _inner = inner;
        _logger = logger;
    }

    public async Task<ErrorOr<IReadOnlyCollection<AttachmentDto>>> Handle(int ticketId, int messageId, CancellationToken cancellationToken)
    {
        try
        {
            return await _inner.Handle(ticketId, messageId, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            if (ex.StatusCode is System.Net.HttpStatusCode.NotFound)
            {
                return Error.NotFound("GetMessageAttachmentsHandler.NotFound", $"Ticket {ticketId} messageId {messageId} attachments not found.");
            }

            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {name}", nameof(GetMessageAttachmentsHandler));
            return Error.Failure("GetMessageAttachmentsHandler.Failure", ex.Message);
        }
    }
}
