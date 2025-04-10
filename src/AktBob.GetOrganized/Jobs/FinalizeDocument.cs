﻿using AktBob.GetOrganized.Contracts;
using AktBob.Shared;
using AktBob.Shared.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace AktBob.GetOrganized.Jobs;

internal record class FinalizeDocumentJob(int DocumentId, bool ShouldCloseOpenTasks = false);

internal class FinalizeDocument(IServiceScopeFactory serviceScopeFactory) : IJobHandler<FinalizeDocumentJob>
{
    private readonly IServiceScopeFactory serviceScopeFactory = serviceScopeFactory;

    public async Task Handle(FinalizeDocumentJob job, CancellationToken cancellationToken = default)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredServiceOrThrow<IFinalizeDocumentHandler>();
        await handler.Handle(job.DocumentId, job.ShouldCloseOpenTasks, cancellationToken);
    }
}