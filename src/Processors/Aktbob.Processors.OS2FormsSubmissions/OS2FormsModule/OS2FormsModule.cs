using Aktbob.Processors.OS2FormsSubmissions.OS2FormsModule.Contracts;
using Aktbob.Processors.OS2FormsSubmissions.OS2FormsModule.Handlers.GetSubmission;
using Aktbob.Processors.OS2FormsSubmissions.OS2FormsModule.Handlers.GetSubmissions;
using ErrorOr;

namespace Aktbob.Processors.OS2FormsSubmissions.OS2FormsModule;
internal class OS2FormsModule : IOS2FormsModule
{
    private readonly IGetSubmissionHandler _getSubmissionHandler;
    private readonly IGetSubmissionsHandler _getSubmissionsHandler;

    public OS2FormsModule(IGetSubmissionHandler getSubmissionHandler, IGetSubmissionsHandler getSubmissionsHandler)
    {
        _getSubmissionHandler = getSubmissionHandler;
        _getSubmissionsHandler = getSubmissionsHandler;
    }

    public async Task<ErrorOr<SubmissionDto>> GetSubmission(Guid id, string webformId, CancellationToken cancellationToken = default) => await _getSubmissionHandler.Handle(id, webformId, cancellationToken);
    public async Task<ErrorOr<IReadOnlyCollection<Guid>>> GetSubmissions(string webformId, CancellationToken cancellationToken = default) => await _getSubmissionsHandler.Handle(webformId, cancellationToken);
}
