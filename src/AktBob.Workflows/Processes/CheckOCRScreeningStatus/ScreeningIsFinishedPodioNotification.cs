using AktBob.Podio.Contracts;

namespace AktBob.Workflows.Processes.CheckOCRScreeningStatus;
internal class ScreeningIsFinishedPodioNotification : IJobHandler<ScreeningIsFinishedPodioNotificationJob>
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public ScreeningIsFinishedPodioNotification(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    public Task Handle(ScreeningIsFinishedPodioNotificationJob job, CancellationToken cancellationToken = default)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var podio = scope.ServiceProvider.GetRequiredService<IPodioModule>();

        var commentText = "Screening af dokumenterne er færdig.";
        podio.PostComment(job.PodioItemId, commentText );

        return Task.CompletedTask;
    }
}
