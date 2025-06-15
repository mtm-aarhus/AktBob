using AktBob.Shared.Extensions;
using AktBob.Shared.Types.Podio;

namespace Aktbob.Modules.Podio.Features.UpdateTextField;

internal class UpdateTextFieldHandlerLogging(IUpdateTextFieldHandler next, ILogger<UpdateTextFieldHandler> logger)
    : IUpdateTextFieldHandler
{
    public async Task<ErrorOr<Success>> Handle(ItemId itemId, int fieldId, string textValue, CancellationToken cancellationToken)
    {
        logger.LogInformation("Updating text field {fieldId} on Podio Item {itemId}: '{textValue}'", fieldId, itemId, textValue);
        
        var result = await next.Handle(itemId, fieldId, textValue, cancellationToken);
        
        result.Switch(
            _ => logger.LogInformation("Text field {fieldId} on Podio item {id} updated", fieldId, itemId),
            errors => logger.LogWarning("{name}: {errors}", nameof(UpdateTextFieldHandler), errors.ToCommaDelimitedString()));
        
        return result;
    }
}