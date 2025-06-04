using AktBob.OS2Forms.Contracts;
using AktBob.Shared.Extensions;

namespace AktBob.Workflows.Processes.EnsureSubmissions;

internal class EnsureSubmissionRegistrations(
    IServiceScopeFactory scopeFactory,
    IConfiguration configuration,
    IJobDispatcher jobDispatcher,
    ILogger<EnsureSubmissionRegistrations> logger) : IJobHandler<EnsureSubmissionRegistrationsJob>
{
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private readonly IConfiguration _configuration = configuration;
    private readonly IJobDispatcher _jobDispatcher = jobDispatcher;
    private readonly ILogger<EnsureSubmissionRegistrations> _logger = logger;

    public async Task Handle(EnsureSubmissionRegistrationsJob job, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting list of OS2Forms submissions");
        
        using var scope = _scopeFactory.CreateScope();
        var webformId = Guard.Against.NullOrEmpty(_configuration.GetValue<string>("OS2Forms:WebformId"));
        var os2Forms = scope.ServiceProvider.GetRequiredService<IOS2FormsModule>();
        
        // Get submission ids from OS2Forms
        var submissionIds = await os2Forms.GetSubmissions(webformId, cancellationToken);
        if (submissionIds.IsError) throw new BusinessException($"Error getting submissions from OS2Forms: {submissionIds.Errors.ToCommaDelimitedString()}");
        
        _logger.LogInformation("Currently {count} OS2Forms submissions. Dispatching jobs to check if they have been registered.", submissionIds.Value.Count);
        
        // Enqueue jobs for every ID
        foreach (var submissionId in submissionIds.Value)
        {
            _jobDispatcher.Dispatch(new EnsureSubmissionRegistrationJob(submissionId, 1));
        }
    }
}