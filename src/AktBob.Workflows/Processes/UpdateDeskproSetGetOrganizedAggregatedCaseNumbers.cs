using AktBob.Deskpro.Contracts;
using AktBob.GetOrganized.Contracts;
using AktBob.Shared.Jobs;
using System.Text.Json;

namespace AktBob.Workflows.Processes;

internal class UpdateDeskproSetGetOrganizedAggregatedCaseNumbers(IServiceScopeFactory serviceScopeFactory, IConfiguration configuration) : IJobHandler<UpdateDeskproSetGetOrganizedAggregatedCaseNumbersJob>
{
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;
    private readonly IConfiguration _configuration = configuration;

    public Task Handle(UpdateDeskproSetGetOrganizedAggregatedCaseNumbersJob job, CancellationToken cancellationToken = default)
    {
        var scope = _serviceScopeFactory.CreateScope();
        var getOrganized = scope.ServiceProvider.GetRequiredService<IGetOrganizedModule>();
        var deskpro = scope.ServiceProvider.GetRequiredService<IDeskproModule>();

        var deskproWebhook = Guard.Against.NullOrEmpty(_configuration.GetValue<string>("Deskpro:Webhooks:SetGetOrganizedAggregatedCaseIds"));
        
        var caseIds = new List<string>();
                
        var tasks = job.AggregatedCaseIds.Select(
            async aggregatedCaseId =>
            {
                var result = await getOrganized.GetAggregatedCase(aggregatedCaseId.Trim(), cancellationToken);
                caseIds.AddRange(result);
            }).ToArray();
        
        Task.WaitAll(tasks, cancellationToken);

        if (!caseIds.Any())
        {
            return Task.CompletedTask;
        }

        var payload = new
        {
            DeskproTicketId = job.DeskproTicketId,
            CaseIds = string.Join(",", caseIds)
        };

        var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        deskpro.InvokeWebhook(deskproWebhook, json);
        return Task.CompletedTask;
    }
}
