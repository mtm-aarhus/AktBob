using AktBob.OS2Forms.Contracts;
using AktBob.Shared.Extensions;

namespace AktBob.Workflows.Processes.EnsureSubmissions;

internal class EnsureSubmissionRegistrations : IJobHandler<EnsureSubmissionRegistrationsJob>
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _configuration;
    private readonly IJobDispatcher _jobDispatcher;

    public EnsureSubmissionRegistrations(IServiceScopeFactory scopeFactory, IConfiguration configuration, IJobDispatcher jobDispatcher)
    {
        _scopeFactory = scopeFactory;
        _configuration = configuration;
        _jobDispatcher = jobDispatcher;
    }
    
    public async Task Handle(EnsureSubmissionRegistrationsJob job, CancellationToken cancellationToken = default)
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