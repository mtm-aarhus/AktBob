using ErrorOr;

namespace Aktbob.Processors.OS2FormsSubmissions.OS2FormsModule.Contracts;

public interface IOS2FormsModule
{
    Task<ErrorOr<SubmissionDto>> GetSubmission(Guid id, string webformId, CancellationToken cancellationToken = default);
    Task<ErrorOr<IReadOnlyCollection<Guid>>> GetSubmissions(string webformId, CancellationToken cancellationToken = default);
}