using Aktbob.Modules.Podio.Features.UpdateTextField;
using AktBob.Shared.Types.Podio;
using Ardalis.GuardClauses;
using ErrorOr;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Aktbob.Processors.CheckOcrScreeningStatus.Features.UpdatePodioItem;

internal class UpdatePodioItemHandler(
    ILogger<UpdatePodioItemHandler> logger,
    IConfiguration configuration,
    IUpdateTextFieldHandler podioUpdateTextFieldHandler)
{
    public async Task<ErrorOr<Success>> Run(UpdatePodioItemJob job, CancellationToken cancellationToken)
    {
        var podioAppId = Guard.Against.Null(configuration.GetValue<int>("Podio:AppId"));
        var filArkivCaseIdFieldId = Guard.Against.Null(configuration.GetValue<int>("Podio:Fields:FilArkivCaseId"));
        var filArkivLinkFieldId = Guard.Against.Null(configuration.GetValue<int>("Podio:Fields:FilArkivLink"));

        await Task.WhenAll([
            podioUpdateTextFieldHandler.Handle(ItemId.Create(podioAppId, job.PodioItemId), filArkivCaseIdFieldId, job.FilArkivCaseId.ToString(), cancellationToken),
            podioUpdateTextFieldHandler.Handle(ItemId.Create(podioAppId, job.PodioItemId), filArkivLinkFieldId, $"https://aarhus.filarkiv.dk/archives/case/{job.FilArkivCaseId}", cancellationToken)
        ]);

        return Result.Success;
    }
}