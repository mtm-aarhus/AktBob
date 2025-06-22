using AktBob.Api.Endpoints.Cases;
using AktBob.Api.Endpoints.CleanUpQueues.FilArkiv;
using AktBob.Api.Endpoints.Tickets;
using AktBob.Api.Endpoints.Jobs;
using AktBob.Api.Endpoints.Podio;
using AktBob.Api.Endpoints.Submissions;

namespace AktBob.Api.Endpoints;

internal static class MapEndpoints
{
    public static void MapJobEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/jobs")
            .WithTags("Jobs")
            .RequireAuthorization();

        group.MapPost("/journalize-everything", JournalizeEverything.Endpoint)
            .WithSummary(JournalizeEverything.Summery)
            .WithDescription(JournalizeEverything.Description);
        
        group.MapPost("/create-afgoerelsesskrivelse", CreateAfgørelsesskrivelse.Endpoint)
            .WithSummary(CreateAfgørelsesskrivelse.Summery)
            .WithDescription(CreateAfgørelsesskrivelse.Description);
        
        group.MapToFilArkivEndpoint("/to-filarkiv");
        group.MapToSharepointEndpoint("/to-sharepoint");
        group.MapCreateDocumentListEndpoint("/create-document-list");
        group.MapScheduleCleanupEndpoint("/schedule-cleanup");
    }
    
    public static void MapTicketEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/tickets")
            .WithTags("Tickets")
            .RequireAuthorization();

        group.MapGetTicketsEndpoint("");
        group.MapGetTicketEndpoint("/{id:int}");
        group.MapCreateTicketEndpoint("");
        group.MapUpdateTicketEndpoint("/{id:int}");
    }

    public static void MapCaseEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/cases")
            .WithTags("Cases")
            .RequireAuthorization();
    
        group.MapCreateCaseEndpoint("");
        group.MapGetCaseEndpoint("/{id:int}");
        group.MapGetCasesEndpoint("");
        group.MapUpdateCaseEndpoint("/{id:int}");
    }

    public static void MapSubmissionEndpoints (this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/submissions")
            .WithTags("Submissions")
            .RequireAuthorization();
        
        group.MapSearchSubmissionsEndpoint("");
        group.MapCreateSubmissionEndpoint("");
    }

    public static void MapCleanUpQueueEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/cleanup-queues")
            .WithTags("Cleanup Queues")
            .RequireAuthorization();
        
        group.MapCreateFilArkivFilesEndpoint("/filarkiv/files");
    }

    public static void MapPodioEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/podio")
            .WithTags("Podio")
            .RequireAuthorization();
        
        group.MapUpdatePodioFieldDocumentListEndpoint("/{itemId}/fields/dokumentliste");
        group.MapUpdatePodioFieldDocumentListEndpoint("/{itemId}/dokumentlisteField");
        
        group.MapUpdatePodioFieldSharepointMappeEndpoint("/{itemId}/fields/sharepointmappe");
        group.MapUpdatePodioFieldSharepointMappeEndpoint("/{itemId}/sharepointmappeField");
    }
    
    public static void MapDatabaseEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/database")
            .WithTags("Database")
            .RequireAuthorization();
        
        group.MapGetTicketEndpoint("/tickets/{id:int}"); // Is this still in use?
        group.MapGetCaseEndpoint("/cases/{id:int}"); // Is this still in use?
    }
}