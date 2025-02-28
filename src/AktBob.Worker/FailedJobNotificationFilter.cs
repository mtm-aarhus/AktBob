using AktBob.Email.Contracts;
using AktBob.Shared.CQRS;
using Hangfire.States;
using Hangfire.Storage;

namespace AktBob.Worker;
internal class FailedJobNotificationFilter(ICommandDispatcher commandDispatcher, ILogger<FailedJobNotificationFilter> logger, IConfiguration configuration) : IApplyStateFilter
{
    private readonly ICommandDispatcher _commandDispatcher = commandDispatcher;
    private readonly ILogger<FailedJobNotificationFilter> _logger = logger;
    private readonly IConfiguration _configuration = configuration;

    public async void OnStateApplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
    {
        if (context.NewState is FailedState failedState)
        {
            var jobId = context.BackgroundJob?.Id;
            var exceptionMessage = failedState.Exception?.Message;

            _logger.LogCritical("Job {jobId} failed: {exceptionMessage}", jobId, exceptionMessage);

            var to = _configuration.GetValue<string>("FailedJobNotificationEmailTo");

            if (string.IsNullOrEmpty(to))
            {
                return;
            }

            var subject = $"AktBob.Worker job {jobId} failed";
            var command = new SendEmailCommand(to, subject, exceptionMessage ?? string.Empty, false);
            await _commandDispatcher.Dispatch(command);
        }
    }

    public void OnStateUnapplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
    {
        // No action for when a failed state is removed
    }
}
