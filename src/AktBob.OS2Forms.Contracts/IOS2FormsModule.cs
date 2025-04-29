using Ardalis.Result;

namespace AktBob.OS2Forms.Contracts;

public interface IOS2FormsModule
{
    Task<Result<SubmissionDto>> GetSubmission(Guid id, string webformId, CancellationToken cancellationToken = default);
}