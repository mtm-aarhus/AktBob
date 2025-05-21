using AktBob.Shared;
using AktBob.Shared.Jobs;
using AktBob.Shared.Types.Deskpro;
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
        var ticketId = TicketId.Create(req.DeskproTicketId);
        DispatchCleanUpJobs(ticketId);
        DispatchCleanUpNotificationJobs(ticketId);
        SetDeskproFieldFinishedAt(ticketId);
        await SendNoContentAsync(ct);
    }

    private void SetDeskproFieldFinishedAt(TicketId ticketId)
    {
        var job = new UpdateDeskproSetFærdigbehandletDatoFieldJob(ticketId);
        _jobDispatcher.Dispatch(job);
    }

    private void DispatchCleanUpJobs(TicketId ticketId)
    {
        if (_jobDispatcher.IsJobAlreadyScheduled(typeof(DispatchCleanupJobsJob), ticketId, nameof(DispatchCleanupJobsJob.TicketId)))
        {
            _logger.LogInformation("{type} with identifier {name}: {id} already scheduled.", typeof(DispatchCleanupJobsJob).Name, nameof(DispatchCleanupJobsJob.TicketId), ticketId);
            return;
        }

        var delayDays = Guard.Against.Zero(_configuration.GetValue<int>("CleanUpJobsDelayDays"));
        var offset = new DateTimeOffset(DateTime.UtcNow.AddDays(delayDays).AddHours(1));

        var job = new DispatchCleanupJobsJob(ticketId);
        _jobDispatcher.Dispatch(job, offset);
    }

    private void DispatchCleanUpNotificationJobs(TicketId ticketId)
    {
        if (_jobDispatcher.IsJobAlreadyScheduled(typeof(NotitfyAboutUpcomingCleanupJob), ticketId, nameof(NotitfyAboutUpcomingCleanupJob.TicketId)))
        {
            _logger.LogInformation("{type} with identifier {name}: {id} already scheduled.", typeof(NotitfyAboutUpcomingCleanupJob).Name, nameof(NotitfyAboutUpcomingCleanupJob.TicketId), ticketId);
            return;
        }

        var delayDays = Guard.Against.Zero(_configuration.GetValue<int>("CleanUpJobsNotificationDelayDays"));
        var offset = new DateTimeOffset(DateTime.UtcNow.AddDays(delayDays).AddHours(1));

        var job = new NotitfyAboutUpcomingCleanupJob(ticketId);
        _jobDispatcher.Dispatch(job, offset);
    }
}
