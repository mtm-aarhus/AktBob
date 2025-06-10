using AktBob.Deskpro.Contracts;
using AktBob.Email.Contracts;
using AktBob.Shared.Jobs;
using AktBob.Workflows.Helpers;

namespace AktBob.Workflows.Processes.Cleanup;
internal class NotifyAboutUpcomingCleanup : IJobHandler<NotitfyAboutUpcomingCleanupJob>
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<NotifyAboutUpcomingCleanup> _logger;

    public NotifyAboutUpcomingCleanup(IServiceScopeFactory serviceScopeFactory, IConfiguration configuration, ILogger<NotifyAboutUpcomingCleanup> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _configuration = configuration;
        _logger = logger;
    }
    public async Task Handle(NotitfyAboutUpcomingCleanupJob job, CancellationToken cancellationToken = default)
    {
        Guard.Against.Zero(job.TicketId);

        // Variables
        var validWorkflowChoices = Guard.Against.Null(_configuration.GetSection("DispatchCleanupJobsHandler:ValidWorkflowChoices").Get<List<int>>());
        var afslutningsdatoFieldId = Guard.Against.Zero(_configuration.GetValue<int>("DispatchCleanupJobsHandler:AfslutningsdatoFieldId"));
        var workflowFieldId = Guard.Against.Zero(_configuration.GetValue<int>("DispatchCleanupJobsHandler:WorkflowFieldId"));

        // Services
        using var scope = _serviceScopeFactory.CreateScope();

        var deskpro = scope.ServiceProvider.GetRequiredService<IDeskproModule>();
        var jobDispatcher = scope.ServiceProvider.GetRequiredService<IJobDispatcher>();
        var email = scope.ServiceProvider.GetRequiredService<IEmailModule>();


        // Check Deskpro to ensure a) it's time to execute, b) the ticket is still closed
        var deskproTicket = await CleanUpShared.GetDeskproTicket(job.TicketId, deskpro, cancellationToken);
        if (deskproTicket.IsError) throw new BusinessException($"Error getting Deskpro ticket {job.TicketId}");

        var workflowFieldValue = CleanUpShared.ParseWorkflowValue(deskproTicket.Value, workflowFieldId);
        var afslutningsdatoFieldValue = CleanUpShared.ParseAfslutningsdatoValue(deskproTicket.Value, afslutningsdatoFieldId);


        // If there is no timestamp in Deskpro or the ticket is not closed, exit the job
        if (workflowFieldValue is null || !validWorkflowChoices.Contains((int)workflowFieldValue))
        {
            _logger.LogWarning("Cannot notify about upcoming deletion since Deskpro ticket {ticketId} is not closed.", job.TicketId);
            return;
        }

        if (afslutningsdatoFieldValue is null)
        {
            _logger.LogWarning("Cannot notify about upcoming deleton since Deskpro ticket {ticketId} does not have a value for afslutningsdato.", job.TicketId);
            return;
        }

        // Cast for convenience
        var afslutningsdato = (DateTime)afslutningsdatoFieldValue;


        // If it's too early, reschedule the job (Deskpro timestamp have changed since the job initially was scheduled)
        if (JobHasToBeRescheduled(job.TicketId, jobDispatcher, afslutningsdato))
        {
            return;
        }

        // Get notification recipient
        var agentId = deskproTicket.Value.Agent?.Id;
        if (agentId == null)
        {
            throw new BusinessException($"Cannot notify about upcoming cleanup. Deskpro ticket {job.TicketId} has no agent.");
        }

        var agent = await deskpro.GetPersonById((int)agentId, cancellationToken);
        if (agent.IsError) throw new BusinessException($"Agent {agentId} not found in Deskpro");

        // Send notification
        var subject = $"{job.TicketId}: Midlertidige dokumenter i screenings- og udleveringsmapperne bliver snart slettet";
        var fields = new Dictionary<string, string>
        {
            { "ticketId", job.TicketId.ToString() },
            { "ticketSubject", deskproTicket.Value.Subject }
        };

        var emailBody = HtmlHelper.GenerateHtml(fields, "EmailTemplates/upcoming-clean-up-notification.html");
        email.Send(agent.Value.Email, subject, emailBody, true);
    }

    private bool JobHasToBeRescheduled(int ticketId, IJobDispatcher jobDispatcher, DateTime afslutningsdato)
    {
        var executionDelayDays = Guard.Against.Zero(_configuration.GetValue<int>("DispatchCleanupJobsHandler:NotificationDelayDays"));
        var utcNow = DateTimeOffset.UtcNow;

        var offset = new DateTimeOffset(afslutningsdato.AddDays(executionDelayDays));
        var daysDifference = offset - utcNow;

        if (DateTime.UtcNow >= afslutningsdato.AddDays(executionDelayDays))
        {
            _logger.LogInformation("Notification about upcoming deletion for Deskpro ticket {ticketId} due.", ticketId);
            return false;
        }

        _logger.LogInformation("Rescheduling job notifying about upcoming cleanup for Deskpro ticket {ticketId}. Try next in {days} days.", ticketId, Math.Round(daysDifference.TotalDays, 2));

        var rescheduledJob = new NotitfyAboutUpcomingCleanupJob(ticketId);
        jobDispatcher.Dispatch(rescheduledJob, offset.AddHours(1));

        return true;
    }
}
