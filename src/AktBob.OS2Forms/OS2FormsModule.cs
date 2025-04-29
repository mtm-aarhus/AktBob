using AktBob.OS2Forms.Contracts;
using Ardalis.Result;

namespace AktBob.OS2Forms;
internal class OS2FormsModule : IOS2FormsModule
{
    private readonly IGetSubmissionHandler _getSubmissionHandler;

    public OS2FormsModule(IGetSubmissionHandler getSubmissionHandler)
    {
        _getSubmissionHandler = getSubmissionHandler;
    }

    public async Task<Result<SubmissionDto>> GetSubmission(Guid id, string webformId, CancellationToken cancellationToken = default) => await _getSubmissionHandler.Handle(id, webformId, cancellationToken);
}
