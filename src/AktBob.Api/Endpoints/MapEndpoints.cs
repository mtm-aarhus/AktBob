using AktBob.Api.Endpoints.Cases;
using AktBob.Api.Endpoints.CleanUpQueues.FilArkiv;
using AktBob.Api.Endpoints.Tickets;
using AktBob.Api.Endpoints.Jobs;
using AktBob.Api.Endpoints.Submissions;
using AktBob.Shared.Contracts.Database;
using Scalar.AspNetCore;

namespace AktBob.Api.Endpoints;

internal static class MapEndpoints
{
    public static IEndpointRouteBuilder MapJobEndpoints(this IEndpointRouteBuilder endpoints)
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
        
        return endpoints;
    }
    
    public static IEndpointRouteBuilder MapTicketEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/tickets")
            .WithTags("Tickets")
            .RequireAuthorization();

        group.MapGetTicketsEndpoint("");
        group.MapGetTicketEndpoint("/{id:int}");
        group.MapCreateTicketEndpoint("");
        group.MapUpdateTicketEndpoint("/{id:int}");
        
        return endpoints;
    }

    public static IEndpointRouteBuilder MapCaseEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/cases")
            .WithTags("Cases")
            .RequireAuthorization();
    
        group.MapCreateCaseEndpoint("");
        group.MapGetCaseEndpoint("/{id:int}");
        group.MapGetCasesEndpoint("");
        group.MapUpdateCaseEndpoint("/{id:int}");
    
        return group;
    }

    public static IEndpointRouteBuilder MapSubmissionEndpoints (this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/submissions")
            .WithTags("Submissions")
            .RequireAuthorization();
        
        group.MapSearchSubmissionsEndpoint("");
        group.MapCreateSubmissionEndpoint("");
        
        return group;
    }

    public static IEndpointRouteBuilder MapCleanUpQueueEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/cleanup-queues")
            .WithTags("Cleanup Queues")
            .RequireAuthorization();
        
        group.MapCreateFilArkivFilesEndpoint("/filarkiv/files");
        
        return endpoints;
    }
    
    public static IEndpointRouteBuilder MapDatabaseEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/database")
            .WithTags("Database")
            .RequireAuthorization();
        
        group.MapGetTicketEndpoint("/tickets/{id:int}"); // Is this still in use?
        group.MapGetCaseEndpoint("/cases/{id:int}"); // Is this still in use?
        
        return endpoints;
    }
}