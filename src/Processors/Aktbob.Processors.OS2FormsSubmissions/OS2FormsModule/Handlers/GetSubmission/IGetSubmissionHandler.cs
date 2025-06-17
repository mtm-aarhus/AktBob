using Aktbob.Processors.OS2FormsSubmissions.OS2FormsModule.Contracts;
using ErrorOr;

namespace Aktbob.Processors.OS2FormsSubmissions.OS2FormsModule.Handlers.GetSubmission;

internal interface IGetSubmissionHandler
{
    Task<ErrorOr<SubmissionDto>> Handle(Guid submissionId, string webformId, CancellationToken cancellationToken);
}