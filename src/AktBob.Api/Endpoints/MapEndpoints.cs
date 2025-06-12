using AktBob.Api.Endpoints.Tickets;
using AktBob.Api.Endpoints.Jobs;
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
        
        group.MapGet("/{id:int}", GetTicket.Endpoint).WithSummary(GetTicket.Summery).WithDescription(GetTicket.Description).Produces<TicketDto>();
        group.MapGet("", GetTickets.Endpoint).WithSummary(GetTickets.Description).WithSummary(GetTickets.Summery).Produces<TicketDto[]>();
        
        group.MapPost("", CreateTicket.Endpoint).WithSummary(CreateTicket.Summery).WithDescription(CreateTicket.Description).Produces(StatusCodes.Status204NoContent);
        
        group.MapPatch("/{id:int}", UpdateTicket.Endpoint).WithSummary(UpdateTicket.Summery).WithDescription(UpdateTicket.Description).Produces(StatusCodes.Status204NoContent);
        
        return endpoints;
    }

    public static IEndpointRouteBuilder MapDatabaseEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/database")
            .WithTags("Database")
            .RequireAuthorization();
        
        group.MapPatch("/tickets/{id:int}", UpdateTicket.Endpoint).WithSummary(UpdateTicket.Summery).WithDescription(UpdateTicket.Description);
        group.MapGet("/tickets/{id:int}", GetTicket.Endpoint).WithSummary(GetTicket.Summery).WithDescription(GetTicket.Description).Produces<TicketDto>(StatusCodes.Status200OK);
        group.MapGet("/tickets", GetTickets.Endpoint).WithSummary(GetTickets.Description).WithSummary(GetTickets.Summery).Produces<TicketDto[]>();
        
        return endpoints;
    }
}