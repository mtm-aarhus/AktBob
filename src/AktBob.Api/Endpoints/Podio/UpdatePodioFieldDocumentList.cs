using AktBob.Shared.Extensions;
using AktBob.Shared.ModuleClients.PodioModule;
using Microsoft.AspNetCore.Mvc;

namespace AktBob.Api.Endpoints.Podio;

internal static class UpdatePodioFieldDocumentList
{
    public static void MapUpdatePodioFieldDocumentListEndpoint(this RouteGroupBuilder builder, string route) => builder
        .MapPut(route, EndpointHandler)
        .WithSummary("Opdater dokumentlistefelt i Podio")
        .WithDescription("Opdaterer dokumentlistefeltet i Podio med den angivne string value")
        .Produces(StatusCodes.Status204NoContent);
    
    private static async Task<IResult> EndpointHandler(
        [FromRoute] long itemId,
        [FromBody] UpdateFieldRequest request,
        [FromServices] IConfiguration configuration,
        [FromServices] IPodioModuleClient podio,
        CancellationToken cancellationToken)
    {
        var appId = configuration.GetValue<int>("Podio:AktindsigtApp:Id");
        var fieldId = configuration.GetValue<int>("Podio:AktindsigtApp:Fields:Dokumentliste");

        var updateRequest = new AktBob.Shared.Contracts.Modules.Podio.UpdateFieldRequest(fieldId, request.Value);
        var result = await podio.UpdateField(appId, itemId, updateRequest, cancellationToken);
        return result.ToMinimalApiResponse();
    }
}