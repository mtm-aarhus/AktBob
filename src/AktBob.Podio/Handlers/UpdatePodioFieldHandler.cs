using AAK.Podio;
using AktBob.Podio.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AktBob.Podio.Handlers;
internal class UpdatePodioFieldHandler(IPodioFactory podioFactory, ILogger<UpdatePodioFieldHandler> logger, IConfiguration configuration) : IUpdatePodioFieldHandler
{
    private readonly IPodioFactory _podioFactory = podioFactory;
    private readonly ILogger<UpdatePodioFieldHandler> _logger = logger;
    private readonly IConfiguration _configuration = configuration;

    public async Task Handle(int appId, long itemId, int fieldId, string value, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating Podio item. ItemId {itemId} FielId {fieldId} Value: '{value}'", itemId, fieldId, value);

        var podio = _podioFactory.Create(
            appId: appId,
            appToken: ConfigurationHelper.GetAppToken(_configuration, appId),
            clientId: ConfigurationHelper.GetClientId(_configuration),
            clientSecret: ConfigurationHelper.GetClientSecret(_configuration));

        await podio.UpdateItemField(appId, itemId, fieldId, value, cancellationToken);

        _logger.LogInformation("Podio item updated. ItemId {itemId} FieldId {fieldId} Value: '{value}'", itemId, fieldId, value);
    }
}