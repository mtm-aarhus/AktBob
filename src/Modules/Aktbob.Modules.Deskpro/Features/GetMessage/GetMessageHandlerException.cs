using AktBob.Shared.Contracts.Modules.Deskpro.DTOs;

namespace Aktbob.Modules.Deskpro.Features.GetMessage;
internal class GetMessageHandlerException(IGetMessageHandler inner, ILogger<GetMessageHandler> logger)
    : IGetMessageHandler
{
    private readonly IGetMessageHandler _inner = inner;
    private readonly ILogger<GetMessageHandler> _logger = logger;

    public async Task<ErrorOr<MessageDto>> Handle(int ticketId, int messageId, CancellationToken cancellationToken)
    {
        try
        {
            return await _inner.Handle(ticketId, messageId, cancellationToken);
        }
        catch (HttpRequestException ex)
        when (ex.StatusCode == System.Net.HttpStatusCode.NotFound) 
        {
            return Error.NotFound("DeskproGetMessageHandler.NotFound", $"Message {messageId} not found.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {name}", nameof(GetMessageHandler));
            return Error.Failure("GetMessageHandler.Failure", ex.Message);
        }
    }
}
