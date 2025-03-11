using Hangfire.States;
using Hangfire.Storage;

namespace AktBob.Worker;
internal class FailedJobLoggingFilter(ILogger<FailedJobLoggingFilter> logger) : IApplyStateFilter
{
    private readonly ILogger<FailedJobLoggingFilter> _logger = logger;

    public void OnStateApplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
    {
        if (context.NewState is FailedState failedState)
        {
            var jobId = context.BackgroundJob?.Id;
            var methodName = context.BackgroundJob?.Job.ToString();
            var exceptionType = failedState.Exception.GetType().Name;
            var exceptionMessage = failedState.Exception?.Message;
            var args = string.Join(", ", context.BackgroundJob?.Job.Args ?? Enumerable.Empty<object>());

            _logger.LogCritical("Job {id} failed. {name}({args}). {exceptionType}: {exceptionMessage}", jobId, methodName, args, exceptionType, exceptionMessage);
        }
    }

    public void OnStateUnapplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
    {
        // No action for when a failed state is removed
    }
}
