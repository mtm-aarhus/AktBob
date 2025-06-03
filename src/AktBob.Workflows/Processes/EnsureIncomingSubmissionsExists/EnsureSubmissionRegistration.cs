using AktBob.Database.Contracts;
using AktBob.Deskpro.Contracts;
using AktBob.OS2Forms.Contracts;
using AktBob.Shared.Extensions;

namespace AktBob.Workflows.Processes.EnsureIncomingSubmissionsExists;

internal record EnsureSubmissionRegistrationJob(Guid SubmissionId, int Count);

internal class EnsureSubmissionRegistration : IJobHandler<EnsureSubmissionRegistrationJob>
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IJobDispatcher _jobDispatcher;
    private readonly IConfiguration _configuration;
    private readonly ILogger<EnsureSubmissionRegistrationJob> _logger;

    public EnsureSubmissionRegistration(IServiceScopeFactory scopeFactory, IJobDispatcher jobDispatcher, IConfiguration configuration, ILogger<EnsureSubmissionRegistrationJob> logger)
    {
        _scopeFactory = scopeFactory;
        _jobDispatcher = jobDispatcher;
        _configuration = configuration;
        _logger = logger;
    }
    
    public async Task Handle(EnsureSubmissionRegistrationJob job, CancellationToken cancellationToken = default)
    {
        using var scope = _scopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IOS2FormsSubmissionRepository>();
        var os2Forms = scope.ServiceProvider.GetRequiredService<IOS2FormsModule>();
        var deskpro = scope.ServiceProvider.GetRequiredService<IDeskproModule>();
        
        // Check if submission has been registered in database
        var submission = await repository.GetBySubmissionId(job.SubmissionId);
        if (submission is not null && submission.DeskproTicketId > 0)
        {
            // Submission has been registered -> do nothing
            return;
        }
        
        // Submission has not been registered in database yet -> check Deskpro
        var deskproTicket = await deskpro.GetTicketsByFieldSearch([191], job.SubmissionId.ToString(), cancellationToken);
        if (deskproTicket is { IsError: false, Value.Count: > 0 })
        {
            switch (deskproTicket.Value.Count)
            {
                case 1:
                    // One Deskpro ticket is registered with the submission ID -> all is good
                    _logger.LogWarning("OS2Forms submission {id} is not yet registered in database but has been registered in Deskpro, ticket ID {ticketId}", job.SubmissionId, deskproTicket.Value.First().Id);
                    return;
                
                case > 1:
                    // Multiple Deskpro tickets exists for the specified OS2Forms submission ID -> weird.
                    _logger.LogError("OS2Forms submission {id} has been registered to multiple Deskpro tickets: {tickets}", job.SubmissionId, string.Join(", ", deskproTicket.Value.Select(x => x.Id)));
                    return;
            }
        }
        
        // Submission not yet registered -> check again maximum 3 times then fail
        var count = job.Count + 1;
        if (count > 3)
        {
            var webformId = Guard.Against.NullOrEmpty(_configuration.GetValue<string>("OS2Forms:WebformId"));
            var os2FormsSubmission = await os2Forms.GetSubmission(job.SubmissionId, webformId, cancellationToken);
            if (os2FormsSubmission.IsError) throw new BusinessException($"OS2Forms submission {job.SubmissionId} not registered in Deskpro and also not found in OS2Forms! Error: {os2FormsSubmission.Errors.ToCommaDelimitedString()}");
            throw new BusinessException($"OS2Forms submission {job.SubmissionId} not registered in Deskpro!");
        }

        var schedule = TimeSpan.FromMinutes(2);
        _logger.LogWarning("OS2Forms submission {SubmissionId} not yet registered in Deskpro. Rechecking in {time}, count: {count}", job.SubmissionId, schedule, count);
        _jobDispatcher.Dispatch(job with { Count = count }, schedule);
    }
}