using AktBob.Shared;
using AktBob.Shared.Contracts.Processors;
using AktBob.Shared.Extensions;
using Microsoft.AspNetCore.Mvc;
using Ardalis.GuardClauses;

namespace AktBob.Api.Endpoints.JournalizeEverything;

internal static class JournalizeEverything
{
    public static string Description => "Journaliserer alle udleverede dokumenter til aktindsigtssagen i GetOrganized (via proces i OpenOrchestrator).";
    
    public static async Task<IResult> Endpoint(
        [FromServices] IConfiguration configuration,
        [FromServices] IMessageBus messageBus,
        [FromBody] JournalizeEverythingRequest request,
        CancellationToken cancellationToken)
    {
        var queueName = Guard.Against.NullOrEmpty(configuration.GetValue<string>("Queues:JournalizeEverything"));
        var job = new JournalizeEverythingJob(request.DeskproId);
        var result = await messageBus.SendMessage(queueName, job, cancellationToken);
        return result.ToMinimalApiResponse();
    }
}
