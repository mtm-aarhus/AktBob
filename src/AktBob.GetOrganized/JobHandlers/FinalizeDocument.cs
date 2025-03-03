using AktBob.GetOrganized.Contracts;
using AktBob.GetOrganized.Contracts.Jobs;
using AktBob.Shared;
using Microsoft.Extensions.DependencyInjection;

namespace AktBob.GetOrganized.JobHandlers;
internal class FinalizeDocument(IServiceScopeFactory serviceScopeFactory) : IJobHandler<FinalizeDocumentJob>
{
    private readonly IServiceScopeFactory serviceScopeFactory = serviceScopeFactory;

    public async Task Handle(FinalizeDocumentJob job, CancellationToken cancellationToken = default)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var finalizeGetOrganizedDocumentsHandler = scope.ServiceProvider.GetRequiredService<IFinalizeGetOrganizedDocumentHandler>();
        await finalizeGetOrganizedDocumentsHandler.Handle(job.DocumentId, false, cancellationToken);
    }
}