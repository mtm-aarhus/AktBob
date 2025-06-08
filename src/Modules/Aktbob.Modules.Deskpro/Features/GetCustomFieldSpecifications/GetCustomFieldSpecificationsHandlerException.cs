using AktBob.Shared.Contracts.Modules.Deskpro.DTOs;

namespace Aktbob.Modules.Deskpro.Features.GetCustomFieldSpecifications;
internal class GetCustomFieldSpecificationsHandlerException(
    IGetCustomFieldSpecificationsHandler inner,
    ILogger<GetCustomFieldSpecificationsHandler> logger)
    : IGetCustomFieldSpecificationsHandler
{
    private readonly IGetCustomFieldSpecificationsHandler _inner = inner;
    private readonly ILogger<GetCustomFieldSpecificationsHandler> _logger = logger;

    public async Task<ErrorOr<IReadOnlyCollection<CustomFieldSpecificationDto>>> Handle(CancellationToken cancellationToken)
    {
        try
        {
            return await _inner.Handle(cancellationToken);
        }
        catch (HttpRequestException ex)
        when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return Error.NotFound("GetCustomerFieldSpecificationsHandler.NotFound", $"Custom field specification from Deskpro not found: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {name}", nameof(GetCustomFieldSpecificationsHandler));
            return Error.Failure("GetCustomFieldSpecificationsHandler.Failure", ex.Message);
        }
    }
}
