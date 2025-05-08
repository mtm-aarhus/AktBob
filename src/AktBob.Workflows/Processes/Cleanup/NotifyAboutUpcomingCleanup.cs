using AktBob.Deskpro.Contracts;
using AktBob.Deskpro.Contracts.DTOs;
using AktBob.Email.Contracts;
using AktBob.Shared.Jobs;
using AktBob.Workflows.Helpers;
using Microsoft.AspNetCore.Mvc.Rendering;

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
        Guard.Against.Zero(job.DeskproTicketId);

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
        TicketDto deskproTicket = await CleanUpShared.GetDeskproTicket(job.DeskproTicketId, deskpro, cancellationToken);
        var workflowFieldValue = CleanUpShared.ParseWorkflowValue(deskproTicket, workflowFieldId);
        var afslutningsdatoFieldValue = CleanUpShared.ParseAfslutningsdatoValue(deskproTicket, afslutningsdatoFieldId);


        // If there is no timestamp in Deskpro or the ticket is not closed, exit the job
        if (workflowFieldValue is null || !validWorkflowChoices.Contains((int)workflowFieldValue))
        {
            _logger.LogWarning("Cannot notify about upcoming deletion since Deskpro ticket {ticketId} is not closed.", job.DeskproTicketId);
            return;
        }

        if (afslutningsdatoFieldValue is null)
        {
            _logger.LogWarning("Cannot notify about upcoming deleton since Deskpro ticket {ticketId} does not have a value for afslutningsdato.", job.DeskproTicketId);
            return;
        }

        // Cast for convenience
        var afslutningsdato = (DateTime)afslutningsdatoFieldValue;


        // If it's too early, reschedule the job (Deskpro timestamp have changed since the job initially was scheduled)
        if (JobHasToBeRescheduled(job.DeskproTicketId, jobDispatcher, afslutningsdato))
        {
            return;
        }

        // Get notification recipient
        var personId = deskproTicket.Agent?.Id;
        if (personId == null)
        {
            throw new BusinessException($"Cannot notify about upcoming cleanup. Deskpro ticket {job.DeskproTicketId} has no agent.");
        }

        var person = await deskpro.GetPerson((int)personId, cancellationToken);
        if (!person.IsSuccess)
        {
            throw new BusinessException($"Agent {personId} not found in Deskpro");
        }

        // Send notification
        var recipient = person.Value.Email;
        var subject = $"{job.DeskproTicketId}: Midlertidige dokumenter i screenings- og udleveringsmapperne bliver snart slettet";
        var fields = new Dictionary<string, string>
        {
            { "ticketId", job.DeskproTicketId.ToString() },
            { "ticketSubject", deskproTicket.Subject }
        };

        var emailBody = HtmlHelper.GenerateHtml(fields, "EmailTemplates/upcoming-clean-up-notification.html");
        email.Send(recipient, subject, emailBody, true);
    }

    private bool JobHasToBeRescheduled(int deskproTicketId, IJobDispatcher jobDispatcher, DateTime afslutningsdato)
    {
        var executionDelayDays = Guard.Against.Zero(_configuration.GetValue<int>("DispatchCleanupJobsHandler:NotificationDelayDays"));
        var utcNow = DateTimeOffset.UtcNow;

        var offset = new DateTimeOffset(afslutningsdato.AddDays(executionDelayDays));
        var daysDifference = offset - utcNow;

        if (DateTime.UtcNow >= afslutningsdato.AddDays(executionDelayDays))
        {
            _logger.LogInformation("Notification about upcoming deletion for Deskpro ticket {ticketId} due.", deskproTicketId);
            return false;
        }

        _logger.LogInformation("Rescheduling job notifying about upcoming cleanup for Deskpro ticket {ticketId}. Try next in {days} days.", deskproTicketId, Math.Round(daysDifference.TotalDays, 2));

        var rescheduledJob = new NotitfyAboutUpcomingCleanupJob(deskproTicketId);
        jobDispatcher.Dispatch(rescheduledJob, offset.AddHours(1));

        return true;
    }
}
