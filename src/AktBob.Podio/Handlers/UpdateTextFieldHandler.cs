using AAK.Podio;
using AktBob.Podio.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AktBob.Podio.Handlers;
internal class UpdateTextFieldHandler(IPodioFactory podioFactory, ILogger<UpdateTextFieldHandler> logger, IConfiguration configuration) : IUpdateTextFieldHandler
{
    private readonly IPodioFactory _podioFactory = podioFactory;
    private readonly ILogger<UpdateTextFieldHandler> _logger = logger;
    private readonly IConfiguration _configuration = configuration;

    public async Task Handle(UpdateTextFieldCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating Podio item. ItemId {itemId} FielId {fieldId} Value: '{value}'", command.PodioItemId.Id, command.FieldId, command.TextValue);

        var podio = _podioFactory.Create(
            appId: command.PodioItemId.AppId,
            appToken: ConfigurationHelper.GetAppToken(_configuration, command.PodioItemId.AppId),
            clientId: ConfigurationHelper.GetClientId(_configuration),
            clientSecret: ConfigurationHelper.GetClientSecret(_configuration));

        await podio.UpdateItemField(command.PodioItemId.AppId, command.PodioItemId.Id, command.FieldId, command.TextValue, cancellationToken);

        _logger.LogInformation("Podio item updated. ItemId {itemId} FieldId {fieldId} Value: '{value}'", command.PodioItemId.Id, command.FieldId, command.TextValue);
    }
}