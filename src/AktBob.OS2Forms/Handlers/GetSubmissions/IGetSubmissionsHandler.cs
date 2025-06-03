using ErrorOr;

namespace AktBob.OS2Forms.Handlers.GetSubmissions;

internal interface IGetSubmissionsHandler
{
    Task<ErrorOr<IReadOnlyCollection<Guid>>> Handle(string webformId, CancellationToken cancellationToken);
}