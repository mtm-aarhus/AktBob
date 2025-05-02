using AktBob.Shared;
using AktBob.Shared.Jobs;
using Ardalis.GuardClauses;
using FastEndpoints;

namespace AktBob.Api.Endpoints.CreateCleanUpQueueItems;

internal class CreateCleanUpQueueItemsEndpoint : Endpoint<CreateCleanUpQueueItemsRequest>
{
    private readonly IJobDispatcher _jobDispatcher;
    private readonly IConfiguration _configuration;
    private readonly ILogger<CreateCleanUpQueueItemsEndpoint> _logger;

    public CreateCleanUpQueueItemsEndpoint(IJobDispatcher jobDispatcher, IConfiguration configuration, ILogger<CreateCleanUpQueueItemsEndpoint> logger)
    {
        _jobDispatcher = jobDispatcher;
        _configuration = configuration;
        _logger = logger;
    }

    public override void Configure()
    {
        Post("/Jobs/CreateCleanUpQueueItems");
        Options(x => x.WithTags("Jobs"));
        Summary(s =>
        {
            s.Summary = "Dispatches OpenOrchestrator queue items for cleanup processes";
        });
    }

    public override async Task HandleAsync(CreateCleanUpQueueItemsRequest req, CancellationToken ct)
    {
        DispatchCleanUpJobs(req.DeskproTicketId);
        DispatchCleanUpNotificationJobs(req.DeskproTicketId);
        await SendNoContentAsync(ct);
    }

    private void DispatchCleanUpJobs(int deskproTicketId)
    {
        if (_jobDispatcher.IsJobAlreadyScheduled(typeof(DispatchCleanupJobsJob), deskproTicketId, nameof(DispatchCleanupJobsJob.DeskproTicketId)))
        {
            _logger.LogInformation("{type} with identifier {name}: {id} already scheduled.", typeof(DispatchCleanupJobsJob).Name, nameof(DispatchCleanupJobsJob.DeskproTicketId), deskproTicketId);
            return;
        }

        var delayDays = Guard.Against.Zero(_configuration.GetValue<int>("CleanUpJobsDelayDays"));
        var offset = new DateTimeOffset(DateTime.UtcNow.AddDays(delayDays).AddHours(1));

        var job = new DispatchCleanupJobsJob(deskproTicketId);
        _jobDispatcher.Dispatch(job, offset);
    }

    private void DispatchCleanUpNotificationJobs(int deskproTicketId)
    {
        if (_jobDispatcher.IsJobAlreadyScheduled(typeof(NotitfyAboutUpcomingCleanupJob), deskproTicketId, nameof(NotitfyAboutUpcomingCleanupJob.DeskproTicketId)))
        {
            _logger.LogInformation("{type} with identifier {name}: {id} already scheduled.", typeof(NotitfyAboutUpcomingCleanupJob).Name, nameof(NotitfyAboutUpcomingCleanupJob.DeskproTicketId), deskproTicketId);
            return;
        }

        var delayDays = Guard.Against.Zero(_configuration.GetValue<int>("CleanUpJobsNotificationDelayDays"));
        var offset = new DateTimeOffset(DateTime.UtcNow.AddDays(delayDays).AddHours(1));

        var job = new NotitfyAboutUpcomingCleanupJob(deskproTicketId);
        _jobDispatcher.Dispatch(job, offset);
    }
}
