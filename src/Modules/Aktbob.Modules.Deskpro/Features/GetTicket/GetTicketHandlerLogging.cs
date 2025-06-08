using AktBob.Shared.Contracts.Modules.Deskpro.DTOs;
using AktBob.Shared.Extensions;
using AktBob.Shared.Types.Deskpro;

namespace Aktbob.Modules.Deskpro.Features.GetTicket;
internal class GetTicketHandlerLogging(IGetTicketHandler inner, ILogger<GetTicketHandler> logger) : IGetTicketHandler
{
    private readonly IGetTicketHandler _inner = inner;
    private readonly ILogger<GetTicketHandler> _logger = logger;

    public async Task<ErrorOr<TicketDto>> Handle(TicketId ticketId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting Deskpro ticket {id}", ticketId);

        var result = await _inner.Handle(ticketId, cancellationToken);

        result.Switch(
            _ => _logger.LogInformation("Deskpro ticket {id} retrieved", ticketId),
            errors => _logger.LogWarning("{name}: {errors}", nameof(AktBob.Deskpro.Handlers.GetTicket), errors.ToCommaDelimitedString()));

        return result;
    }
}
