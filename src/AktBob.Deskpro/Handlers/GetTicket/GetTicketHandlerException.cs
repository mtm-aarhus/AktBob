using System.Net;
using AktBob.Deskpro.Contracts.DTOs;

namespace AktBob.Deskpro.Handlers.GetTicket;
internal class GetTicketHandlerException : IGetTicketHandler
{
    private readonly IGetTicketHandler _inner;
    private readonly ILogger<GetTicketHandler> _logger;

    public GetTicketHandlerException(IGetTicketHandler inner, ILogger<GetTicketHandler> logger)
    {
        _inner = inner;
        _logger = logger;
    }

    public async Task<ErrorOr<TicketDto>> Handle(int ticketId, CancellationToken cancellationToken)
    {
        try
        {
            return await _inner.Handle(ticketId, cancellationToken);
        }
        catch (HttpRequestException ex)
        when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return Error.NotFound("GetTicketHandler.NotFound", $"Deskpro ticket {ticketId} not found.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {name}", nameof(GetTicketHandler));
            return Error.Failure("GetTicketHandler.Failure", ex.Message);
        }
    }
}
