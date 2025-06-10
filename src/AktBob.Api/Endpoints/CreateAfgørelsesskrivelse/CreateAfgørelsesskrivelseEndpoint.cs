using AktBob.Shared;
using AktBob.Shared.Contracts.Processors;
using AktBob.Shared.Extensions;
using Ardalis.GuardClauses;
using Microsoft.AspNetCore.Mvc;

namespace AktBob.Api.Endpoints.CreateAfgørelsesskrivelse;

internal static class CreateAfgørelsesskrivelseEndpoint
{
    public static IEndpointRouteBuilder MapCreateAfgørelsesskrivelseEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/jobs/create-afgoerelsesskrivelse", async (
            [FromServices] IConfiguration configuration,
            [FromServices] IMessageBus messageBus,
            [FromBody] CreateAfgørelsesskrivelseRequest request,
            CancellationToken cancellationToken) =>
        {
            var queueName = Guard.Against.NullOrEmpty(configuration.GetValue<string>("Queues:CreateAfgoerelsesskrivelse"));
            var job = new CreateAfgørelsesskrivelseJob(request.DeskproId);
            var result = await messageBus.SendMessage(queueName, job, cancellationToken);
            return result.ToMinimalApiResponse();
        });
            
        return endpoints;
    }
    
}