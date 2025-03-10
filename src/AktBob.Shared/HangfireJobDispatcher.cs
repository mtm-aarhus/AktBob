using Hangfire;

namespace AktBob.Shared;

public class HangfireJobDispatcher(IBackgroundJobClient backgroundJobClient) : IJobDispatcher
{
    private readonly IBackgroundJobClient _backgroundJobClient = backgroundJobClient;

    public void Dispatch<TJob>(TJob job) where TJob : class
    {
        _backgroundJobClient.Enqueue<IJobHandler<TJob>>(handler => handler.Handle(job, CancellationToken.None));
    }

    public void Dispatch<TJob>(TJob job, TimeSpan delay) where TJob : class
    {
        _backgroundJobClient.Schedule<IJobHandler<TJob>>(handler => handler.Handle(job, CancellationToken.None), delay);
    }
}
