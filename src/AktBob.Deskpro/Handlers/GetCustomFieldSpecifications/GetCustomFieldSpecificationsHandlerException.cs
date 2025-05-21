namespace AktBob.Deskpro.Handlers.GetCustomFieldSpecifications;
internal class GetCustomFieldSpecificationsHandlerException : IGetCustomFieldSpecificationsHandler
{
    private readonly IGetCustomFieldSpecificationsHandler _inner;
    private readonly ILogger<GetCustomFieldSpecificationsHandler> _logger;

    public GetCustomFieldSpecificationsHandlerException(IGetCustomFieldSpecificationsHandler inner, ILogger<GetCustomFieldSpecificationsHandler> logger)
    {
        _inner = inner;
        _logger = logger;
    }
    public async Task<ErrorOr<IReadOnlyCollection<CustomFieldSpecificationDto>>> Handle(CancellationToken cancellationToken)
    {
        try
        {
            return await _inner.Handle(cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            if (ex.StatusCode is System.Net.HttpStatusCode.NotFound)
            {
                return Error.NotFound("GetCustomerFieldSpecificationsHandler.NotFound", $"Custom field specification from Deskpro not found: {ex.Message}");
            }

            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {name}", nameof(GetCustomFieldSpecificationsHandler));
            return Error.Failure("GetCustomFieldSpecificationsHandler.Failure", ex.Message);
        }
    }
}
