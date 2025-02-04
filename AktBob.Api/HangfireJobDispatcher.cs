using AktBob.Shared;
using Hangfire;

namespace AktBob.Api;

public class HangfireJobDispatcher(IBackgroundJobClient backgroundJobClient, IServiceProvider serviceProvider) : IJobDispatcher
{
    private readonly IBackgroundJobClient _backgroundJobClient = backgroundJobClient;

    public IServiceProvider ServiceProvider { get; } = serviceProvider;

    public void Dispatch<TJob>(TJob job) where TJob : class
    {
        _backgroundJobClient.Enqueue<IJobHandler<TJob>>(handler => handler.Handle(job, CancellationToken.None));
    }
}
