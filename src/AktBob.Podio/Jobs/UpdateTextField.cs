using AktBob.Podio.Handlers.UpdateTextField;
using AktBob.Shared;
using AktBob.Shared.Extensions;
using AktBob.Shared.Types.Podio;
using Ardalis.GuardClauses;
using Microsoft.Extensions.DependencyInjection;

namespace AktBob.Podio.Jobs;

internal record UpdateTextFieldJob(ItemId ItemId, int FieldId, string TextValue);

internal class UpdateTextField(IServiceScopeFactory serviceScopeFactory) : IJobHandler<UpdateTextFieldJob>
{
    public async Task Handle(UpdateTextFieldJob job, CancellationToken cancellationToken = default)
    {
        Guard.Against.NegativeOrZero(job.ItemId.AppId);
        Guard.Against.NegativeOrZero(job.ItemId.Id);
        Guard.Against.NegativeOrZero(job.FieldId);
        Guard.Against.NullOrEmpty(job.TextValue);

        using var scope = serviceScopeFactory.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredServiceOrThrow<IUpdateTextFieldHandler>();

        await handler.Handle(job.ItemId, job.FieldId, job.TextValue, cancellationToken);
    }
}