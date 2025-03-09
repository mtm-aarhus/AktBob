using AktBob.Podio.Contracts;
using AktBob.Shared;
using Ardalis.GuardClauses;
using Microsoft.Extensions.DependencyInjection;

namespace AktBob.Podio.Jobs;

internal record UpdateTextFieldJob(PodioItemId PodioItemId, int FieldId, string TextValue);

internal class UpdateTextField(IServiceScopeFactory serviceScopeFactory) : IJobHandler<UpdateTextFieldJob>
{
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;

    public async Task Handle(UpdateTextFieldJob job, CancellationToken cancellationToken = default)
    {
        Guard.Against.NegativeOrZero(job.PodioItemId.AppId);
        Guard.Against.NegativeOrZero(job.PodioItemId.Id);
        Guard.Against.NegativeOrZero(job.FieldId);
        Guard.Against.NullOrEmpty(job.TextValue);

        using var scope = _serviceScopeFactory.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<IUpdateTextFieldHandler>();

        var command = new UpdateTextFieldCommand(job.PodioItemId, job.FieldId, job.TextValue);
        await handler.Handle(command, cancellationToken);
    }
}