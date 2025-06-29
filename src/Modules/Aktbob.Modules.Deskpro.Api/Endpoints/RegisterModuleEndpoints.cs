namespace Aktbob.Modules.Deskpro.Api.Endpoints;

internal static class RegisterModuleEndpoints
{
    public static IEndpointRouteBuilder MapModuleEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/download-message-attachment", EndpointsHandlers.DownloadMessageAttachment);
        endpoints.MapGet("/custom-field-specifications", EndpointsHandlers.GetCustomFieldSpecifications);
        
        endpoints.MapGet("/search/tickets", EndpointsHandlers.SearchTicketsByFields);
        
        endpoints.MapGet("/tickets/{id:int}", EndpointsHandlers.GetTicket);
        endpoints.MapGet("/tickets/{ticketId:int}/messages", EndpointsHandlers.GetMessages);
        endpoints.MapGet("/tickets/{ticketId:int}/messages/{messageId:int}", EndpointsHandlers.GetMessage);
        endpoints.MapGet("/tickets/{ticketId:int}/messages/{messageId:int}/attachments", EndpointsHandlers.GetMessageAttachments);
        
        endpoints.MapGet("/persons", EndpointsHandlers.GetPersonByEmail);
        endpoints.MapGet("/persons/{id:int}", EndpointsHandlers.GetPersonById);
        
        endpoints.MapGet("/teams/{id:int}", EndpointsHandlers.GetTeam);
        
        endpoints.MapPost("/webhook/{webhookId}", EndpointsHandlers.InvokeWebhook);
        
        return endpoints;
    }
}