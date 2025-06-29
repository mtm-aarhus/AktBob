using Aktbob.Modules.Podio.Features.PostComment;
using Aktbob.Processors.CheckOcrScreeningStatus.Contracts;
using AktBob.Shared.Types.Podio;
using Ardalis.GuardClauses;
using ErrorOr;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Aktbob.Processors.CheckOcrScreeningStatus.Features.PodioNotification;

internal class PodioNotificationHandler(
    ILogger<PodioNotificationHandler> logger,
    IConfiguration configuration,
    IPostCommentHandler podioPostCommentHandler)
{
    public async Task<ErrorOr<Success>> Run(
        PodioNotificationJob job,
        CancellationToken cancellationToken)
    {
        var podioAppId = Guard.Against.Null(configuration.GetValue<int?>("Podio:AppId"));
        await podioPostCommentHandler.Handle(ItemId.Create(podioAppId, job.PodioItemId), "Screening af dokumenterne er færdig.", cancellationToken);
        
        return Result.Success;
    }
}