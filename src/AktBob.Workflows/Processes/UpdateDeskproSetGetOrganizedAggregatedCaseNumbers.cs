using AktBob.GetOrganized.Contracts;
using AktBob.Shared.Extensions;
using System.Text.Json;
using Aktbob.Modules.Deskpro.Features.InvokeWebhook;
using AktBob.Shared.Contracts.Processors;

namespace AktBob.Workflows.Processes;

internal class UpdateDeskproSetGetOrganizedAggregatedCaseNumbers(IServiceScopeFactory serviceScopeFactory, IConfiguration configuration) : IJobHandler<UpdateDeskproSetGetOrganizedAggregatedCaseNumbersJob>
{
    public async Task Handle(UpdateDeskproSetGetOrganizedAggregatedCaseNumbersJob job, CancellationToken cancellationToken = default)
    {
        if (job.AggregatedCaseIds.Length == 0)
        {
            return;
        }

        Guard.Against.NegativeOrZero(job.TicketId);

        using var scope = serviceScopeFactory.CreateScope();
        var getOrganized = scope.ServiceProvider.GetRequiredServiceOrThrow<IGetOrganizedModule>();
        var deskproInvokeWebhookHandler = scope.ServiceProvider.GetRequiredServiceOrThrow<IInvokeWebhookHandler>();

        var deskproWebhook = Guard.Against.NullOrEmpty(configuration.GetValue<string>("Deskpro:Webhooks:SetGetOrganizedAggregatedCaseIds"));
        
        var caseIds = new List<string>();
                
        var tasks = job.AggregatedCaseIds.Select(
            async aggregatedCaseId =>
            {
                var result = await getOrganized.GetAggregatedCase(aggregatedCaseId.Trim(), cancellationToken);
                caseIds.AddRange(result.Value);
            }).ToArray();
        
        await Task.WhenAll(tasks);

        if (!caseIds.Any())
        {
            return;
        }

        var payload = new
        {
            DeskproTicketId = job.TicketId,
            CaseIds = string.Join(",", caseIds)
        };

        var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        await deskproInvokeWebhookHandler.Handle(deskproWebhook, json, cancellationToken);
    }
}
