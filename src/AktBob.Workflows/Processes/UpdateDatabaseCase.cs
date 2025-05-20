using AktBob.Database.Contracts;
using AktBob.Shared.Jobs;

namespace AktBob.Workflows.Processes;
internal class UpdateDatabaseCase : IJobHandler<UpdateDatabaseCaseJob>
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<UpdateDatabaseCase> _logger;

    public UpdateDatabaseCase(IServiceScopeFactory scopeFactory, ILogger<UpdateDatabaseCase> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task Handle(UpdateDatabaseCaseJob job, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating database case {id}", job.Id);

        using var scope = _scopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ICaseRepository>();

        // Get existing case from repository
        var @case = await repository.Get(job.Id);
        if (@case == null) throw new BusinessException($"Error updating database case {job.Id}: Not found in database.");

        // Update case properties
        if (!string.IsNullOrEmpty(job.CaseNumber))
        {
            @case.CaseNumber = job.CaseNumber;
        }

        if (!string.IsNullOrEmpty(job.SharepointFolderName))
        {
            @case.SharepointFolderName = job.SharepointFolderName;
        }

        @case.PodioItemId = job.PodioItemId ?? @case.PodioItemId;
        @case.FilArkivCaseId = job.FilArkivCaseId ?? @case.FilArkivCaseId;


        // Update entity
        var updated = await repository.Update(@case);


        // Response
        if (updated)
        {
            _logger.LogInformation("Database case {id} updated: {case}", job.Id, @case);
            return;
        }

        _logger.LogError("Error updating database case {id}", job.Id);
    }
}
