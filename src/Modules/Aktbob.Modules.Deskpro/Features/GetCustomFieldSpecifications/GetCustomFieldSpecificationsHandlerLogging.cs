using Aktbob.Modules.Deskpro.Contracts.DTOs;
using AktBob.Shared.Extensions;

namespace Aktbob.Modules.Deskpro.Features.GetCustomFieldSpecifications;
internal class GetCustomFieldSpecificationsHandlerLogging(
    IGetCustomFieldSpecificationsHandler inner,
    ILogger<GetCustomFieldSpecificationsHandler> logger) : IGetCustomFieldSpecificationsHandler
{
    private readonly IGetCustomFieldSpecificationsHandler _inner = inner;
    private readonly ILogger<GetCustomFieldSpecificationsHandler> _logger = logger;

    public async Task<ErrorOr<IReadOnlyCollection<CustomFieldSpecificationDto>>> Handle(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting Deskpro custom field specifications");

        var result = await _inner.Handle(cancellationToken);

        result.Switch(
            _ => _logger.LogInformation("Deskpro custom field specifications retrieved"),
            errors => _logger.LogWarning("{name}: {errors}", nameof(GetCustomFieldSpecificationsHandler), errors.ToCommaDelimitedString()));

        return result;
    }
}
