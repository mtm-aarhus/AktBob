using AktBob.Deskpro.Contracts.DTOs;

namespace AktBob.Deskpro.Handlers.GetMessages;
internal class GetMessagesHandlerException : IGetMessagesHandler
{
    private readonly IGetMessagesHandler _inner;
    private readonly ILogger<GetMessagesHandler> _logger;

    public GetMessagesHandlerException(IGetMessagesHandler inner, ILogger<GetMessagesHandler> logger)
    {
        _inner = inner;
        _logger = logger;
    }

    public async Task<ErrorOr<IReadOnlyCollection<MessageDto>>> Handle(int ticketId, CancellationToken cancellationToken)
    {
        try
        {
            return await _inner.Handle(ticketId, cancellationToken);
        }
        catch (HttpRequestException ex)
        when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return Error.NotFound("GetMessagesHandler.NotFound", $"Messages for ticket {ticketId} not found.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {name}", nameof(GetMessagesHandler));
            return Error.Failure("GetMessagesHandler.Failure", ex.Message);
        }
    }
}
