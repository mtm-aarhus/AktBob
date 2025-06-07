using AktBob.Shared.Extensions;

namespace Aktbob.Modules.Deskpro.Features.GetMessages;
internal class GetMessagesHandlerLogging(IGetMessagesHandler inner, ILogger<GetMessagesHandler> logger) : IGetMessagesHandler
{
    private readonly IGetMessagesHandler _inner = inner;
    private readonly ILogger<GetMessagesHandler> _logger = logger;

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
