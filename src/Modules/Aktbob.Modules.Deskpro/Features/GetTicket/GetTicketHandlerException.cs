using System.Net;
using AktBob.Shared.Contracts.Modules.Deskpro.DTOs;

namespace Aktbob.Modules.Deskpro.Features.GetTicket;
internal class GetTicketHandlerException(IGetTicketHandler inner, ILogger<GetTicketHandler> logger) : IGetTicketHandler
{
    private readonly IGetTicketHandler _inner = inner;
    private readonly ILogger<GetTicketHandler> _logger = logger;

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
