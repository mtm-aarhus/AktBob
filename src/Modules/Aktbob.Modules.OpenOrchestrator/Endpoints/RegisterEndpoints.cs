namespace Aktbob.Modules.OpenOrchestrator.Endpoints;

internal static class RegisterEndpoints
{
    public static IEndpointRouteBuilder MapModuleEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/queue-item", EndpointHandlers.AddQueueItem);
        return endpoints;
    }
}