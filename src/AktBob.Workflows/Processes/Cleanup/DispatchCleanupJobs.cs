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
        Guard.Against.Zero(job.DeskproTicketId);

        // Variables
        var validWorkflowChoices = Guard.Against.Null(_configuration.GetSection("DispatchCleanupJobsHandler:ValidWorkflowChoices").Get<List<int>>());
        var afslutningsdatoFieldId = Guard.Against.Zero(_configuration.GetValue<int>("DispatchCleanupJobsHandler:AfslutningsdatoFieldId"));
        var workflowFieldId = Guard.Against.Zero(_configuration.GetValue<int>("DispatchCleanupJobsHandler:WorkflowFieldId"));

        // Services
        using var scope = _serviceScopeFactory.CreateScope();

        var deskpro = scope.ServiceProvider.GetRequiredService<IDeskproModule>();
        var jobDispatcher = scope.ServiceProvider.GetRequiredService<IJobDispatcher>();

        // Check Deskpro to ensure a) it's time to execute, b) the ticket is still closed
        var deskproTicket = await CleanUpShared.GetDeskproTicket(job.DeskproTicketId, deskpro, cancellationToken);
        if (deskproTicket.IsError) throw new BusinessException($"Error getting Deskpro ticket {job.DeskproTicketId}");

        var workflowFieldValue =  CleanUpShared.ParseWorkflowValue(deskproTicket.Value, workflowFieldId);
        var afslutningsdatoFieldValue = CleanUpShared.ParseAfslutningsdatoValue(deskproTicket.Value, afslutningsdatoFieldId);


        // If there is no timestamp in Deskpro or the ticket is not closed, exit the job
        if (workflowFieldValue is null || !validWorkflowChoices.Contains((int)workflowFieldValue))
        {
            _logger.LogWarning("Cannot create cleanup queue items since Deskpro ticket {ticketId} is not closed.", job.DeskproTicketId);
            return;
        }

        if (afslutningsdatoFieldValue is null)
        {
            _logger.LogWarning("Cannot create cleanup queue items since Deskpro ticket {ticketId} does not have a value for afslutningsdato.", job.DeskproTicketId);
            return;
        }

        // Cast for convenience
        var afslutningsdato = (DateTime)afslutningsdatoFieldValue;


        // If it's too early, reschedule the job (Deskpro timestamp have changed since the job initially was scheduled)
        if (JobHasBeenRescheduled(job.DeskproTicketId, jobDispatcher, afslutningsdato))
        {
            return;
        }

        // Create the cleanup jobs
        jobDispatcher.Dispatch(new CreateCleanupSharepointQueueItemJob(job.DeskproTicketId));
        jobDispatcher.Dispatch(new CreateCleanupFilArkivQueueItemJob(job.DeskproTicketId));
    }


    private bool JobHasBeenRescheduled(int deskproTicketId, IJobDispatcher jobDispatcher, DateTime afslutningsdato)
    {
        var executionDelayDays = Guard.Against.Zero(_configuration.GetValue<int>("DispatchCleanupJobsHandler:ExecutionDelayDays"));
        var utcNow = DateTimeOffset.UtcNow;

        var offset = new DateTimeOffset(afslutningsdato.AddDays(executionDelayDays));
        var daysDifference = offset - utcNow;

        if (DateTime.UtcNow >= afslutningsdato.AddDays(executionDelayDays))
        {
            _logger.LogInformation("Creating of cleanup job for Deskpro ticket {ticketId} due.", deskproTicketId);
            return false;
        }


        _logger.LogInformation("Rescheduling cleanup job for Deskpro ticket {ticketId}. Next try in {days} days.", deskproTicketId, Math.Round(daysDifference.TotalDays, 2));

        var rescheduledJob = new DispatchCleanupJobsJob(deskproTicketId);
        jobDispatcher.Dispatch(rescheduledJob, offset.AddHours(1));
        
        return true;
    }    
}
