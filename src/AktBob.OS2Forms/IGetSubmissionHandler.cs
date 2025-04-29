using AktBob.OS2Forms.Contracts;
using Ardalis.Result;

namespace AktBob.OS2Forms;

internal interface IGetSubmissionHandler
{
    Task<Result<SubmissionDto>> Handle(Guid submissionId, string webformId, CancellationToken cancellationToken);
}