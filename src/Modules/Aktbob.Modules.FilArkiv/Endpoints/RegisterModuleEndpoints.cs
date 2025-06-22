namespace Aktbob.Modules.FilArkiv.Endpoints;

internal static class RegisterModuleEndpoints
{
    public static IEndpointRouteBuilder MapModuleEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/cases/{caseId:guid}/documents", EndpointHandlers.GetDocumentsByCaseId);
            endpoints.MapGet("/files/{fileId:guid}/processing-status", EndpointHandlers.GetFileProcessStatus);
        return endpoints;
    }
}