using AktBob.GetOrganized.Contracts;
using AktBob.Shared;
using Microsoft.Extensions.DependencyInjection;

namespace AktBob.GetOrganized.Jobs;

internal record class FinalizeDocumentJob(int DocumentId);

internal class FinalizeDocument(IServiceScopeFactory serviceScopeFactory) : IJobHandler<FinalizeDocumentJob>
{
    private readonly IServiceScopeFactory serviceScopeFactory = serviceScopeFactory;

    public async Task Handle(FinalizeDocumentJob job, CancellationToken cancellationToken = default)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<IFinalizeDocumentHandler>();
        await handler.Handle(job.DocumentId, false, cancellationToken);
    }
}