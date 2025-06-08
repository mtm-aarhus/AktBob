using AktBob.Deskpro.Contracts.DTOs;
using AktBob.Shared.Types.Deskpro;

namespace AktBob.Deskpro.Handlers.GetMessage;
internal class GetMessageHandlerException : IGetMessageHandler
{
    private readonly IGetMessageHandler _inner;
    private readonly ILogger<GetMessageHandler> _logger;

    public GetMessageHandlerException(IGetMessageHandler inner, ILogger<GetMessageHandler> logger)
    {
        _inner = inner;
        _logger = logger;
    }

    public async Task<ErrorOr<MessageDto>> Handle(MessageId messageId, CancellationToken cancellationToken)
    {
        try
        {
            return await _inner.Handle(messageId, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            if (ex.StatusCode is System.Net.HttpStatusCode.NotFound)
            {
                return Error.NotFound("DeskproGetMessangeHandler.NotFound", $"Message {messageId} not found.");
            }

            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {name}", nameof(GetMessageHandler));
            return Error.Failure("GetMessageHandler.Failure", ex.Message);
        }
    }
}
