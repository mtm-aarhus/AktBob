using AktBob.GetOrganized.Contracts;
using AktBob.Shared.Extensions;
using AktBob.Shared.Jobs;

namespace AktBob.Workflows.Processes;
internal class UpdateGetOrganizedCaseSetKleValue : IJobHandler<UpdateGetOrganizedCaseSetKleValueJob>
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _configuration;

    public UpdateGetOrganizedCaseSetKleValue(IServiceScopeFactory scopeFactory, IConfiguration configuration)
    {
        _scopeFactory = scopeFactory;
        _configuration = configuration;
    }


    public async Task Handle(UpdateGetOrganizedCaseSetKleValueJob job, CancellationToken cancellationToken = default)
    {
        using var scope = _scopeFactory.CreateScope();
        var go = scope.ServiceProvider.GetRequiredService<IGetOrganizedModule>();


        // Get the target GetOrganized case
        var getTargetCase = await go.GetCaseMetadata(job.TargetCaseId, cancellationToken);
        if (getTargetCase.IsError) throw new BusinessException($"Error getting metadata for GetOrganized case {job.TargetCaseId}");


        // If target case already has a KLE value assigned - do nothing
        if (getTargetCase.Value.KleId != Guid.Empty)
        {
            return;
        }


        // If source case is a Nova-case: set KLE
        if (job.SourceCaseId.IsNovaCase())
        {
            var kleValue = Guard.Against.NullOrEmpty(_configuration.GetValue<Guid>("UpdateGetOrganizedCaseSetKleValue:NovaCaseKleMapping"));
            await UpdateTargetCaseKle(go, job.TargetCaseId, kleValue, cancellationToken);
            return;
        }


        // Get source case from GO
        var sourceCase = await go.GetCaseMetadata(job.SourceCaseId, cancellationToken);
        if (sourceCase.IsError) throw new BusinessException($"Error getting GetOrganized case {job.SourceCaseId} metadata");


        // Set target case KLE using KLE value from source case
        await UpdateTargetCaseKle(go, job.TargetCaseId, sourceCase.Value.KleId, cancellationToken);        
    }

    private async Task UpdateTargetCaseKle(IGetOrganizedModule go, string caseId, Guid kleId, CancellationToken cancellationToken)
    {
        var result = await go.UpdateCaseMetadata(caseId, kleId, cancellationToken);
        if (result.IsError) throw new BusinessException($"Error update GetOrganized case {caseId} metadata");
    }
}
