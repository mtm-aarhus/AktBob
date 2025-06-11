using AktBob.Api.Endpoints.Jobs;
using AktBob.Api.Endpoints.Tickets;

namespace AktBob.Api.Endpoints;

internal static class MapEndpoints
{
    public static IEndpointRouteBuilder MapJobEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/jobs")
            .WithTags("Jobs")
            .RequireAuthorization();

        group.MapPost("/journalize-everything", JournalizeEverything.Endpoint).WithSummary(JournalizeEverything.Summery).WithDescription(JournalizeEverything.Description);
        group.MapPost("/create-afgoerelsesskrivelse", CreateAfgørelsesskrivelse.Endpoint).WithSummary(CreateAfgørelsesskrivelse.Summery).WithDescription(CreateAfgørelsesskrivelse.Description);

        return endpoints;
    }
    
    public static IEndpointRouteBuilder MapTicketEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/tickets")
            .WithTags("Tickets")
            .RequireAuthorization();
        
        group.MapPost("", CreateTicket.Endpoint).WithSummary("Create ticket").WithDescription(CreateTicket.Description);
        return endpoints;
    } 
}