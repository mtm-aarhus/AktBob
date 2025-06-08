using AktBob.Shared.Contracts.Modules.Deskpro.DTOs;

namespace Aktbob.Modules.Deskpro.Features.GetMessages;
internal class GetMessagesHandlerException(IGetMessagesHandler inner, ILogger<GetMessagesHandler> logger) : IGetMessagesHandler
{
    private readonly IGetMessagesHandler _inner = inner;
    private readonly ILogger<GetMessagesHandler> _logger = logger;

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
