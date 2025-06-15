using AAK.Podio;
using AktBob.Shared.Types.Podio;

namespace Aktbob.Modules.Podio.Features.UpdateTextField;
internal class UpdateTextFieldHandler(IPodioFactory podioFactory, IConfiguration configuration) : IUpdateTextFieldHandler
{
    public async Task<ErrorOr<Success>> Handle(ItemId itemId, int fieldId, string textValue, CancellationToken cancellationToken)
    {
        var podio = podioFactory.Create(
            appId: itemId.AppId,
            appToken: ConfigurationHelper.GetAppToken(configuration, itemId.AppId),
            clientId: ConfigurationHelper.GetClientId(configuration),
            clientSecret: ConfigurationHelper.GetClientSecret(configuration));

        await podio.UpdateItemField(itemId.AppId, itemId.Id, fieldId, textValue, cancellationToken);
        return Result.Success;
    }
}