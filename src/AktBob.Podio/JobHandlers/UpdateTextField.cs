using AktBob.Podio.Contracts;
using AktBob.Podio.Contracts.Jobs;
using AktBob.Shared;
using Microsoft.Extensions.DependencyInjection;

namespace AktBob.Podio.JobHandlers;
internal class UpdateTextField(IServiceScopeFactory serviceScopeFactory) : IJobHandler<UpdatePodioTextFieldJob>
{
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;

    public async Task Handle(UpdatePodioTextFieldJob job, CancellationToken cancellationToken = default)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<IUpdatePodioFieldHandler>();
        await handler.Handle(job.AppId, job.ItemId, job.FieldId, job.TextValue, cancellationToken);
    }
}