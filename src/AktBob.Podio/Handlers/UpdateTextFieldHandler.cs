using AAK.Podio;
using AktBob.Podio.Contracts;
using Microsoft.Extensions.Configuration;

namespace AktBob.Podio.Handlers;
internal class UpdateTextFieldHandler(IPodioFactory podioFactory, IConfiguration configuration) : IUpdateTextFieldHandler
{
    private readonly IPodioFactory _podioFactory = podioFactory;
    private readonly IConfiguration _configuration = configuration;

    public async Task Handle(UpdateTextFieldCommand command, CancellationToken cancellationToken)
    {
        var podio = _podioFactory.Create(
            appId: command.PodioItemId.AppId,
            appToken: ConfigurationHelper.GetAppToken(_configuration, command.PodioItemId.AppId),
            clientId: ConfigurationHelper.GetClientId(_configuration),
            clientSecret: ConfigurationHelper.GetClientSecret(_configuration));

        await podio.UpdateItemField(command.PodioItemId.AppId, command.PodioItemId.Id, command.FieldId, command.TextValue, cancellationToken);
    }
}