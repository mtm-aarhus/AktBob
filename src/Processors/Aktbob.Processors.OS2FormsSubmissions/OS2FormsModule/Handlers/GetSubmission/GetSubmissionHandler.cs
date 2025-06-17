using AAK.OS2Forms;
using Aktbob.Processors.OS2FormsSubmissions.OS2FormsModule.Contracts;
using ErrorOr;

namespace Aktbob.Processors.OS2FormsSubmissions.OS2FormsModule.Handlers.GetSubmission;
internal class GetSubmissionHandler(IOS2FormsClient os2Forms) : IGetSubmissionHandler
{
    public async Task<ErrorOr<SubmissionDto>> Handle(Guid submissionId, string webformId, CancellationToken cancellationToken)
    {
        var submission = await os2Forms.GetSubmission(submissionId, webformId, cancellationToken);
        if (submission == null)
        {
            return Error.NotFound("OS2Forms.SubmissionNotFound", $"Submission with ID {submissionId} not found");
        }

        var dto = new SubmissionDto(
            Id: submission.Id,
            WebformId: submission.WebformId,
            ParentTypes: submission.ParentTypes,
            Data: submission.Data);

        return dto;           
    }
}
