using AktBob.Api.Endpoints.Cases;
using AktBob.Api.Endpoints.Tickets;
using AktBob.Api.Endpoints.Jobs;
using AktBob.Api.Endpoints.Submissions;
using AktBob.Shared.Contracts.Database;
using Microsoft.OpenApi.MicrosoftExtensions;
using Microsoft.OpenApi.Models;
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
            .WithDescription(JournalizeEverything.Description)
            .Stable();
        
        group.MapPost("/create-afgoerelsesskrivelse", CreateAfgørelsesskrivelse.Endpoint)
            .WithSummary(CreateAfgørelsesskrivelse.Summery)
            .WithDescription(CreateAfgørelsesskrivelse.Description)
            .Stable();

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

    // public static IEndpointRouteBuilder MapCaseEndpoints(this IEndpointRouteBuilder endpoints)
    // {
    //     var group = endpoints.MapGroup("/api/cases")
    //         .WithTags("Cases")
    //         .RequireAuthorization();
    //
    //     group.MapGet("", GetCases.Endpoint).WithSummary(GetCases.Summery).WithDescription(GetCases.Description).Produces<CaseDto[]>();
    //
    //     return group;
    // }

    public static IEndpointRouteBuilder MapSubmissionEndpoints (this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/submissions")
            .WithTags("Submissions")
            .RequireAuthorization();
        
        group.MapSearchSubmissionsEndpoint("");
        return group;
    }
    
    public static IEndpointRouteBuilder MapDatabaseEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/database")
            .WithTags("Database")
            .RequireAuthorization();
        
        group.MapUpdateTicketEndpoint("/tickets/{id:int}");
        group.MapGetTicketEndpoint("/tickets/{id:int}");
        group.MapGetTicketsEndpoint("/tickets");
        
        group.MapGet("/cases", GetCases.Endpoint)
            .WithSummary(GetCases.Summery)
            .WithDescription(GetCases.Description)
            .Produces<CaseDto[]>();
        
        return endpoints;
    }
}