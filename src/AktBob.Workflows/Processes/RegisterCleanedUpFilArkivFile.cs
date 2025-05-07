using AktBob.Database.Contracts;
using AktBob.Shared.Jobs;

namespace AktBob.Workflows.Processes;
internal class RegisterCleanedUpFilArkivFile : IJobHandler<RegisterCleanedUpFilArkivFileJob>
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public RegisterCleanedUpFilArkivFile(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task Handle(RegisterCleanedUpFilArkivFileJob job, CancellationToken cancellationToken = default)
    {
        var scope = _serviceScopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IFilArkivFilesCleanUpQueueRepository>();

        await repository.Add(job.FilArkivFileId);
    }
}
