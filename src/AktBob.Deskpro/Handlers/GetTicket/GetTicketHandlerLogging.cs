namespace AktBob.Deskpro.Handlers.GetTicket;
internal class GetTicketHandlerLogging : IGetTicketHandler
{
    private readonly IGetTicketHandler _inner;
    private readonly ILogger<GetTicketHandler> _logger;

    public GetTicketHandlerLogging(IGetTicketHandler inner, ILogger<GetTicketHandler> logger)
    {
        _inner = inner;
        _logger = logger;
    }

    public async Task<ErrorOr<TicketDto>> Handle(int ticketId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting Deskpro ticket {id}", ticketId);

        var result = await _inner.Handle(ticketId, cancellationToken);

        result.Switch(
            _ => _logger.LogInformation("Deskpro ticket {id} retrived", ticketId),
            errors => _logger.LogWarning("{name}: {errors}", nameof(GetTicket), result.Errors));

        return result;
    }
}
