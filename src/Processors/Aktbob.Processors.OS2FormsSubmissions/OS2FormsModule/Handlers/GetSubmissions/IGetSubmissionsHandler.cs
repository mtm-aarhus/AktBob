using ErrorOr;

namespace Aktbob.Processors.OS2FormsSubmissions.OS2FormsModule.Handlers.GetSubmissions;

internal interface IGetSubmissionsHandler
{
    Task<ErrorOr<IReadOnlyCollection<Guid>>> Handle(string webformId, CancellationToken cancellationToken);
}