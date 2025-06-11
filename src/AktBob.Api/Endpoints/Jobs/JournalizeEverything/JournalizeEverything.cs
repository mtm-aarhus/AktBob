using AktBob.Shared;
using AktBob.Shared.Contracts.Processors;
using AktBob.Shared.Extensions;
using Ardalis.GuardClauses;
using Microsoft.AspNetCore.Mvc;

namespace AktBob.Api.Endpoints.Jobs.JournalizeEverything;

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
