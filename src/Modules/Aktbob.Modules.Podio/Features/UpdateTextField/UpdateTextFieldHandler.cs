using AAK.Podio;
using AktBob.Shared.Types.Podio;

namespace Aktbob.Modules.Podio.Features.UpdateTextField;
internal class UpdateTextFieldHandler(IPodioFactory podioFactory, IConfigurationHelper configurationHelper) : IUpdateTextFieldHandler
{
    public async Task<ErrorOr<Success>> Handle(ItemId itemId, int fieldId, string textValue, CancellationToken cancellationToken)
    {
        var podio = podioFactory.Create(
            appId: itemId.AppId, 
            appToken: configurationHelper.GetAppToken(itemId.AppId), 
            clientId: configurationHelper.ClientId,
            clientSecret: configurationHelper.GetClientSecret);

        await podio.UpdateItemField(itemId.AppId, itemId.Id, fieldId, textValue, cancellationToken);
        return Result.Success;
    }
}