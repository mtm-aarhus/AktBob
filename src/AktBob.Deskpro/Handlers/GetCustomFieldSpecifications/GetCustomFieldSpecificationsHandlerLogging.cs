using AktBob.Deskpro.Contracts.DTOs;
using AktBob.Shared.Extensions;

namespace AktBob.Deskpro.Handlers.GetCustomFieldSpecifications;
internal class GetCustomFieldSpecificationsHandlerLogging : IGetCustomFieldSpecificationsHandler
{
    private readonly IGetCustomFieldSpecificationsHandler _inner;
    private readonly ILogger<GetCustomFieldSpecificationsHandler> _logger;

    public GetCustomFieldSpecificationsHandlerLogging(IGetCustomFieldSpecificationsHandler inner, ILogger<GetCustomFieldSpecificationsHandler> logger)
    {
        _inner = inner;
        _logger = logger;
    }

    public async Task<ErrorOr<IReadOnlyCollection<CustomFieldSpecificationDto>>> Handle(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting Deskpro custom field specifications");

        var result = await _inner.Handle(cancellationToken);

        result.Switch(
            _ => _logger.LogInformation("Deskpro custom field specifications retrieved"),
            errors => _logger.LogWarning("{name}: {errors}", nameof(GetCustomFieldSpecificationsHandler), result.Errors.ToCommaDelimitedString()));

        return result;
    }
}
