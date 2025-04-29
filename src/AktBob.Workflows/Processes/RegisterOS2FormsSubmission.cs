using AktBob.Database.Contracts;
using AktBob.Database.Entities;
using AktBob.OS2Forms.Contracts;
using AktBob.Shared.Jobs;

namespace AktBob.Workflows.Processes;
internal class RegisterOS2FormsSubmission(ILogger<RegisterOS2FormsSubmission> logger, IConfiguration configuration, IServiceScopeFactory serviceScopeFactory) : IJobHandler<RegisterOS2FormsSubmissionJob>
{
    private readonly ILogger<RegisterOS2FormsSubmission> _logger = logger;
    private readonly IConfiguration _configuration = configuration;
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;

    public async Task Handle(RegisterOS2FormsSubmissionJob job, CancellationToken cancellationToken)
    {
        Guard.Against.Null(job.SubmissionId);
        Guard.Against.Null(job.DeskproId);
        
        using var scope = _serviceScopeFactory.CreateScope();

        var webformId = Guard.Against.NullOrEmpty(_configuration.GetValue<string>("OS2Forms:WebformId"));
        var descriptionFieldId = Guard.Against.NullOrEmpty(_configuration.GetValue<string>("OS2Forms:DescriptionFieldId"));

        // Services
        var os2Forms = scope.ServiceProvider.GetRequiredService<IOS2FormsModule>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var existingSubmission = await unitOfWork.OS2FormsSubmissions.GetBySubmissionId(job.SubmissionId);
        if (existingSubmission != null)
        {
            // All done
            return;
        }

        var submission = await os2Forms.GetSubmission(job.SubmissionId, webformId, cancellationToken);
        if (submission is null)
        {
            throw new BusinessException($"Unable to get submisison {job.SubmissionId} from OS2Forms.");
        }

        // Save submission to database
        var entity = new OS2FormsSubmission
        {
            SubmissionId = job.SubmissionId,
            DeskproTicketId = job.DeskproId,
            DescriptionFieldValue = submission.Value.Data.FirstOrDefault(x => x.Key == descriptionFieldId).Value ?? string.Empty
        };

        var success = await unitOfWork.OS2FormsSubmissions.Add(entity);
        if (!success)
        {
            throw new BusinessException($"Failure persisting OS2Forms submission {job.SubmissionId}");
        }
    }
}
