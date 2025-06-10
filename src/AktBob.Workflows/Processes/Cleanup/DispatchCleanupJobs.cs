using AktBob.Deskpro.Contracts;
using AktBob.Shared.Jobs;

namespace AktBob.Workflows.Processes.Cleanup;
internal class DispatchCleanupJobs : IJobHandler<DispatchCleanupJobsJob>
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<DispatchCleanupJobs> _logger;

    public DispatchCleanupJobs(IServiceScopeFactory serviceScopeFactory, IConfiguration configuration, ILogger<DispatchCleanupJobs> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task Handle(DispatchCleanupJobsJob job, CancellationToken cancellationToken = default)
    {
        // Variables
        var validWorkflowChoices = Guard.Against.Null(_configuration.GetSection("DispatchCleanupJobsHandler:ValidWorkflowChoices").Get<List<int>>());
        var afslutningsdatoFieldId = Guard.Against.Zero(_configuration.GetValue<int>("DispatchCleanupJobsHandler:AfslutningsdatoFieldId"));
        var workflowFieldId = Guard.Against.Zero(_configuration.GetValue<int>("DispatchCleanupJobsHandler:WorkflowFieldId"));

        // Services
        using var scope = _serviceScopeFactory.CreateScope();

        var deskpro = scope.ServiceProvider.GetRequiredService<IDeskproModule>();
        var jobDispatcher = scope.ServiceProvider.GetRequiredService<IJobDispatcher>();

        // Check Deskpro to ensure a) it's time to execute, b) the ticket is still closed
        var deskproTicket = await CleanUpShared.GetDeskproTicket(job.TicketId, deskpro, cancellationToken);
        if (deskproTicket.IsError) throw new BusinessException($"Error getting Deskpro ticket {job.TicketId}");

        var workflowFieldValue =  CleanUpShared.ParseWorkflowValue(deskproTicket.Value, workflowFieldId);
        var afslutningsdatoFieldValue = CleanUpShared.ParseAfslutningsdatoValue(deskproTicket.Value, afslutningsdatoFieldId);


        // If there is no timestamp in Deskpro or the ticket is not closed, exit the job
        if (workflowFieldValue is null || !validWorkflowChoices.Contains((int)workflowFieldValue))
        {
            _logger.LogWarning("Cannot create cleanup queue items since Deskpro ticket {ticketId} is not closed.", job.TicketId);
            return;
        }

        if (afslutningsdatoFieldValue is null)
        {
            _logger.LogWarning("Cannot create cleanup queue items since Deskpro ticket {ticketId} does not have a value for afslutningsdato.", job.TicketId);
            return;
        }

        // Cast for convenience
        var afslutningsdato = (DateTime)afslutningsdatoFieldValue;


        // If it's too early, reschedule the job (Deskpro timestamp have changed since the job initially was scheduled)
        if (JobHasBeenRescheduled(job.TicketId, jobDispatcher, afslutningsdato))
        {
            return;
        }

        // Create the cleanup jobs
        jobDispatcher.Dispatch(new CreateCleanupSharepointQueueItemJob(job.TicketId));
        jobDispatcher.Dispatch(new CreateCleanupFilArkivQueueItemJob(job.TicketId));
    }


    private bool JobHasBeenRescheduled(int ticketId, IJobDispatcher jobDispatcher, DateTime afslutningsdato)
    {
        var executionDelayDays = Guard.Against.Zero(_configuration.GetValue<int>("DispatchCleanupJobsHandler:ExecutionDelayDays"));
        var utcNow = DateTimeOffset.UtcNow;

        var offset = new DateTimeOffset(afslutningsdato.AddDays(executionDelayDays));
        var daysDifference = offset - utcNow;

        if (DateTime.UtcNow >= afslutningsdato.AddDays(executionDelayDays))
        {
            _logger.LogInformation("Creating of cleanup job for Deskpro ticket {ticketId} due.", ticketId);
            return false;
        }


        _logger.LogInformation("Rescheduling cleanup job for Deskpro ticket {ticketId}. Next try in {days} days.", ticketId, Math.Round(daysDifference.TotalDays, 2));

        var rescheduledJob = new DispatchCleanupJobsJob(ticketId);
        jobDispatcher.Dispatch(rescheduledJob, offset.AddHours(1));
        
        return true;
    }    
}
