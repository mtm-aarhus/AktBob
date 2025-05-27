using AktBob.Shared.Extensions;
using AktBob.Shared.Types.Podio;
using Microsoft.Extensions.Logging;

namespace AktBob.Podio.Handlers.UpdateTextField;

internal class UpdateTextFieldHandlerLogging : IUpdateTextFieldHandler
{
    private readonly IUpdateTextFieldHandler _next;
    private readonly ILogger<UpdateTextFieldHandler> _logger;

    public UpdateTextFieldHandlerLogging(IUpdateTextFieldHandler next, ILogger<UpdateTextFieldHandler> logger)
    {
        _next = next;
        _logger = logger;
    }
    
    public async Task<ErrorOr<Success>> Handle(ItemId itemId, int fieldId, string textValue, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating text field {fieldId} on Podio Item {itemId}: '{textValue}'", fieldId, itemId, textValue);
        
        var result = await _next.Handle(itemId, fieldId, textValue, cancellationToken);
        
        result.Switch(
            _ => _logger.LogInformation("Text field {fieldId} on Podio item {id} updated", fieldId, itemId),
            errors => _logger.LogWarning("{name}: {errors}", nameof(UpdateTextFieldHandler), errors.ToCommaDelimitedString()));
        
        return result;
    }
}