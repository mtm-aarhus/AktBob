using AktBob.OS2Forms.Contracts;
using AAK.OS2Forms;
using ErrorOr;

namespace AktBob.OS2Forms.Handlers.GetSubmission;
internal class GetSubmissionHandler : IGetSubmissionHandler
{
    private readonly IOS2FormsClient _os2Forms;

    public GetSubmissionHandler(IOS2FormsClient os2Forms)
    {
        _os2Forms = os2Forms;
    }

    public async Task<ErrorOr<SubmissionDto>> Handle(Guid submissionId, string webformId, CancellationToken cancellationToken)
    {
        var submission = await _os2Forms.GetSubmission(submissionId, webformId, cancellationToken);
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
