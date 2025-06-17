using AAK.OS2Forms;
using ErrorOr;

namespace Aktbob.Processors.OS2FormsSubmissions.OS2FormsModule.Handlers.GetSubmissions;

internal class GetSubmissionsHandler(IOS2FormsClient os2Forms) : IGetSubmissionsHandler
{
    public async Task<ErrorOr<IReadOnlyCollection<Guid>>> Handle(string webformId, CancellationToken cancellationToken)
    {
        var result = await os2Forms.GetSubmissions(webformId, cancellationToken);
        return result.ToErrorOr();
    } 
}