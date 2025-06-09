using AktBob.Shared;
using AktBob.Shared.Contracts.Processors;
using AktBob.Shared.Extensions;
using AktBob.Shared.Types.Deskpro;
using Microsoft.AspNetCore.Mvc;
using Ardalis.GuardClauses;

namespace AktBob.Api.Endpoints.JournalizeEverything;

internal static class JournalizeEverythingEndpoint
{
    public static IEndpointRouteBuilder MapJournalizeEverythingEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/jobs/journalize-everything", async (
            [FromServices] IConfiguration configuration,
            [FromServices] IMessageBus messageBus,
            [FromBody] JournalizeEverythingRequest request,
            CancellationToken cancellationToken) =>
        {
            var queueName = Guard.Against.NullOrEmpty(configuration.GetValue<string>("Queues:JournalizeEverything"));
            var ticketId = TicketId.Create(request.DeskproId);
            var job = new JournalizeEverythingJob(ticketId);
            var result = await messageBus.SendMessage(queueName, job, cancellationToken);
            return result.ToMinimalApiResponse();
        });

        return endpoints;
    }
}
