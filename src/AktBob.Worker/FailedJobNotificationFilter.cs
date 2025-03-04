using AktBob.Email.Contracts;
using Hangfire.States;
using Hangfire.Storage;

namespace AktBob.Worker;
internal class FailedJobNotificationFilter(IEmailModule email, ILogger<FailedJobNotificationFilter> logger, IConfiguration configuration) : IApplyStateFilter
{
    private readonly IConfiguration _configuration = configuration;
    private readonly IEmailModule _email = email;
    private readonly ILogger<FailedJobNotificationFilter> _logger = logger;

    public void OnStateApplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
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
            _email.Send(to, subject, exceptionMessage ?? string.Empty);
        }
    }

    public void OnStateUnapplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
    {
        // No action for when a failed state is removed
    }
}
