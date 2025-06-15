using System.Net;
using AktBob.Shared.Types.Podio;

namespace Aktbob.Modules.Podio.Features.UpdateTextField;

internal class UpdateTextFieldHandlerException(IUpdateTextFieldHandler next, ILogger<UpdateTextFieldHandler> logger)
    : IUpdateTextFieldHandler
{
    public async Task<ErrorOr<Success>> Handle(ItemId itemId, int fieldId, string textValue, CancellationToken cancellationToken)
    {
        try
        {
            return await next.Handle(itemId, fieldId, textValue, cancellationToken);
        }
        catch (HttpRequestException ex) when (ex.StatusCode is HttpStatusCode.NotFound)
        {
            logger.LogError("Cannot update text field {fieldId} on Podio Item {id}. Item not found.", fieldId, itemId);
            return Error.NotFound("Podio.ItemNotFound", $"Podio Item {itemId} not found.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in {name}", nameof(UpdateTextFieldHandler));
            return Error.Failure("UpdateTextFieldHandler.Failure", ex.Message);
        }
    }
}