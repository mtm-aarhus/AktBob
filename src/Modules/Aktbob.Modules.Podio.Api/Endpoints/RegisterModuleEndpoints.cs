namespace Aktbob.Modules.Podio.Api.Endpoints;

internal static class RegisterModuleEndpoints
{
    public static IEndpointRouteBuilder MapModuleEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/apps/{appId:int}/items/{itemId:long}", EndpointHandlers.GetItem);
        endpoints.MapPost("/apps/{appId:int}/items/{itemId:long}/comment", EndpointHandlers.PostComment);
        endpoints.MapPatch("/apps/{appId:int}/items/{itemId:long}", EndpointHandlers.UpdateTextField);
        
        return endpoints;
    }
}