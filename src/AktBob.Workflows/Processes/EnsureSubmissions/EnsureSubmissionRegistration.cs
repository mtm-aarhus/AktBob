using AktBob.Database.Contracts;
using AktBob.Deskpro.Contracts;
using AktBob.OS2Forms.Contracts;
using AktBob.Shared.Extensions;

namespace AktBob.Workflows.Processes.EnsureSubmissions;

internal class EnsureSubmissionRegistration(
    IServiceScopeFactory scopeFactory,
    IJobDispatcher jobDispatcher,
    IConfiguration configuration,
    ILogger<EnsureSubmissionRegistrationJob> logger) : IJobHandler<EnsureSubmissionRegistrationJob>
{
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private readonly IJobDispatcher _jobDispatcher = jobDispatcher;
    private readonly IConfiguration _configuration = configuration;
    private readonly ILogger<EnsureSubmissionRegistrationJob> _logger = logger;

    public async Task Handle(EnsureSubmissionRegistrationJob job, CancellationToken cancellationToken = default)
    {
        using var scope = _scopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IOS2FormsSubmissionRepository>();
        var os2Forms = scope.ServiceProvider.GetRequiredService<IOS2FormsModule>();
        var deskpro = scope.ServiceProvider.GetRequiredService<IDeskproModule>();
        var maxCount = _configuration.GetValue<int?>("EnsureSubmissionRegistration:MaxRetries") ?? 3;
        
        // Check if submission has been registered in database
        var submission = await repository.GetBySubmissionId(job.SubmissionId);
        if (submission is not null && submission.DeskproTicketId > 0)
        {
            // Submission has been registered -> do nothing
            _logger.LogInformation("({count}/{max}) OS2Forms submission {id} registration status: registered", job.Count, maxCount, job.SubmissionId);
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
                    _logger.LogWarning("({count}/{max}) OS2Forms submission {id} registration status: not registered in database but is registered in Deskpro, ticket ID {ticketId}", job.Count, maxCount, job.SubmissionId, deskproTicket.Value.First().Id);
                    return;
                
                case > 1:
                    // Multiple Deskpro tickets exists for the specified OS2Forms submission ID -> weird.
                    _logger.LogError("({count}/{max}) OS2Forms submission {id} registration status: is registered to multiple Deskpro tickets: {tickets}", job.Count, maxCount, job.SubmissionId, string.Join(", ", deskproTicket.Value.Select(x => x.Id)));
                    return;
            }
        }
        
        // Submission not yet registered -> check again maximum times then fail
        var count = job.Count + 1;
        if (count > maxCount)
        {
            var webformId = Guard.Against.NullOrEmpty(_configuration.GetValue<string>("OS2Forms:WebformId"));
            var os2FormsSubmission = await os2Forms.GetSubmission(job.SubmissionId, webformId, cancellationToken);
            if (os2FormsSubmission.IsError) throw new BusinessException($"({count}/{maxCount}) OS2Forms submission {job.SubmissionId} registration status: Error requesting submission from OS2Forms: {os2FormsSubmission.Errors.ToCommaDelimitedString()}");
            
            // Max retries -> log critical
            _logger.LogCritical("({count}/{max}) OS2Forms submission {id} registration status: NOT REGISTERED!", job.Count, maxCount, job.SubmissionId);
            return;
        }
        
        var schedule = TimeSpan.FromMinutes(2);
        _logger.LogWarning("({count}/{max}) OS2Forms submission {id} registration status: not registered in database, not registered in Deskpro. Rechecking in {time}", job.Count, maxCount, job.SubmissionId, schedule);
        _jobDispatcher.Dispatch(job with { Count = count }, schedule);
    }
}