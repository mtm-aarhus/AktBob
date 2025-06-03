using ErrorOr;

namespace AktBob.OS2Forms.Contracts;

public interface IOS2FormsModule
{
    Task<ErrorOr<SubmissionDto>> GetSubmission(Guid id, string webformId, CancellationToken cancellationToken = default);
    Task<ErrorOr<IReadOnlyCollection<Guid>>> GetSubmissions(string webformId, CancellationToken cancellationToken = default);
}