using AktBob.OS2Forms.Contracts;
using ErrorOr;

namespace AktBob.OS2Forms.Handlers.GetSubmission;

internal interface IGetSubmissionHandler
{
    Task<ErrorOr<SubmissionDto>> Handle(Guid submissionId, string webformId, CancellationToken cancellationToken);
}