using Hangfire.States;
using Hangfire.Storage;

namespace AktBob.Worker;
internal class FailedJobNotificationFilter(ILogger<FailedJobNotificationFilter> logger) : IApplyStateFilter
{
    private readonly ILogger<FailedJobNotificationFilter> _logger = logger;

    public void OnStateApplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
    {
        if (context.NewState is FailedState failedState)
        {
            var jobId = context.BackgroundJob?.Id;
            var exceptionMessage = failedState.Exception?.Message;

            _logger.LogCritical("Job {jobId} failed: {exceptionMessage}", jobId, exceptionMessage);
        }
    }

    public void OnStateUnapplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
    {
        // No action for when a failed state is removed
    }
}
