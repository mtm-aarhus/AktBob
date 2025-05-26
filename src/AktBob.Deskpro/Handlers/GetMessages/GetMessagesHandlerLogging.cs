using AktBob.Shared.Extensions;

namespace AktBob.Deskpro.Handlers.GetMessages;
internal class GetMessagesHandlerLogging : IGetMessagesHandler
{
    private readonly IGetMessagesHandler _inner;
    private readonly ILogger<GetMessagesHandler> _logger;

    public GetMessagesHandlerLogging(IGetMessagesHandler inner, ILogger<GetMessagesHandler> logger)
    {
        _inner = inner;
        _logger = logger;
    }

    public async Task<ErrorOr<IReadOnlyCollection<MessageDto>>> Handle(int ticketId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting Deskpro ticket {id} messages", ticketId);

        var result = await _inner.Handle(ticketId, cancellationToken);
        result.Switch(
            _ => _logger.LogInformation("Deskpro ticket {id} messages retrieved", ticketId),
            errors => _logger.LogWarning("{name}: {errors}", nameof(GetMessagesHandler), errors.ToCommaDelimitedString()));

        return result;
    }
}
