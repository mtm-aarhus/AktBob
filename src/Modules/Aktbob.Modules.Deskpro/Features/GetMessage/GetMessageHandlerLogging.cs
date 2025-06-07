using Aktbob.Modules.Deskpro.Contracts.DTOs;
using AktBob.Shared.Extensions;
using AktBob.Shared.Types.Deskpro;

namespace Aktbob.Modules.Deskpro.Features.GetMessage;
internal class GetMessageHandlerLogging(IGetMessageHandler inner, ILogger<GetMessageHandler> logger)
    : IGetMessageHandler
{
    private readonly IGetMessageHandler _inner = inner;
    private readonly ILogger<GetMessageHandler> _logger = logger;

    public async Task<ErrorOr<MessageDto>> Handle(MessageId messageId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting Deskpro message {messageId}", messageId);

        var result = await _inner.Handle(messageId, cancellationToken);

        result.Switch(
            _ => _logger.LogInformation("Deskpro message {messageId} retrieved", messageId),
            errors => _logger.LogWarning("{name}: {errors}", nameof(GetMessage), errors.ToCommaDelimitedString()));

        return result;
    }
}
