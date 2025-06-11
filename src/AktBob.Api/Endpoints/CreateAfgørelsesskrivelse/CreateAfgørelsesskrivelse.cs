using AktBob.Shared;
using AktBob.Shared.Contracts.Processors;
using AktBob.Shared.Extensions;
using Ardalis.GuardClauses;
using Microsoft.AspNetCore.Mvc;

namespace AktBob.Api.Endpoints.CreateAfgørelsesskrivelse;

internal static class CreateAfgørelsesskrivelse
{
    public static string Description => "Opretter afgørelsesskrivelsesdokument (via proces i OpenOrchestrator).";
    
    public static async Task<IResult> Endpoint(
        [FromServices] IConfiguration configuration,
        [FromServices] IMessageBus messageBus,
        [FromBody] CreateAfgørelsesskrivelseRequest request,
        CancellationToken cancellationToken)
    {
        var queueName = Guard.Against.NullOrEmpty(configuration.GetValue<string>("Queues:CreateAfgoerelsesskrivelse"));
        var job = new CreateAfgørelsesskrivelseJob(request.DeskproId);
        var result = await messageBus.SendMessage(queueName, job, cancellationToken);
        return result.ToMinimalApiResponse();
    }
}