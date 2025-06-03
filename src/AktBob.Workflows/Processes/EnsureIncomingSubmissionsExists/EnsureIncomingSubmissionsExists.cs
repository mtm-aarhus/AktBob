using AktBob.OS2Forms.Contracts;
using AktBob.Shared.Extensions;
using AktBob.Shared.Jobs;

namespace AktBob.Workflows.Processes.EnsureIncomingSubmissionsExists;

internal class EnsureIncomingSubmissionsExists : IJobHandler<EnsureIncomingSubmissionsExistsJob>
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _configuration;
    private readonly IJobDispatcher _jobDispatcher;

    public EnsureIncomingSubmissionsExists(IServiceScopeFactory scopeFactory, IConfiguration configuration, IJobDispatcher jobDispatcher)
    {
        _scopeFactory = scopeFactory;
        _configuration = configuration;
        _jobDispatcher = jobDispatcher;
    }
    
    public async Task Handle(EnsureIncomingSubmissionsExistsJob job, CancellationToken cancellationToken = default)
    {
        using var scope = _scopeFactory.CreateScope();
        var webformId = Guard.Against.NullOrEmpty(_configuration.GetValue<string>("OS2Forms:WebformId"));
        var os2Forms = scope.ServiceProvider.GetRequiredService<IOS2FormsModule>();
        
        // Get submission ids from OS2Forms
        var submissionIds = await os2Forms.GetSubmissions(webformId, cancellationToken);
        if (submissionIds.IsError) throw new BusinessException($"Error getting submissions from OS2Forms: {submissionIds.Errors.ToCommaDelimitedString()}");
        
        // Enqueue jobs for every ID
        foreach (var submissionId in submissionIds.Value)
        {
            _jobDispatcher.Dispatch(new EnsureSubmissionRegistrationJob(submissionId, 1));
        }
        
    }
}