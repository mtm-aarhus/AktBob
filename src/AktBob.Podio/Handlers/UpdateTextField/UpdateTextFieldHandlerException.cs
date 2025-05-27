using System.Net;
using AktBob.Shared.Types.Podio;
using Microsoft.Extensions.Logging;

namespace AktBob.Podio.Handlers.UpdateTextField;

internal class UpdateTextFieldHandlerException : IUpdateTextFieldHandler
{
    private readonly IUpdateTextFieldHandler _next;
    private readonly ILogger<UpdateTextFieldHandler> _logger;

    public UpdateTextFieldHandlerException(IUpdateTextFieldHandler next, ILogger<UpdateTextFieldHandler> logger)
    {
        _next = next;
        _logger = logger;
    }
    
    public async Task<ErrorOr<Success>> Handle(ItemId itemId, int fieldId, string textValue, CancellationToken cancellationToken)
    {
        try
        {
            return await _next.Handle(itemId, fieldId, textValue, cancellationToken);
        }
        catch (HttpRequestException ex) when (ex.StatusCode is HttpStatusCode.NotFound)
        {
            _logger.LogError("Cannot update text field {fieldId} on Podio Item {id}. Item not found.", fieldId, itemId);
            return Error.NotFound("Podio.ItemNotFound", $"Podio Item {itemId} not found.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {name}", nameof(UpdateTextFieldHandler));
            return Error.Failure("UpdateTextFieldHandler.Failure", ex.Message);
        }
    }
}