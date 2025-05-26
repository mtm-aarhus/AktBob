using AktBob.GetOrganized.Contracts;
using ErrorOr;

namespace AktBob.GetOrganized.Handlers.UploadDocument;
public interface IUploadDocumentHandler
{
    Task<ErrorOr<int>> Handle(
        byte[] bytes,
        string caseNumber,
        string fileName,
        string customProperty,
        DateTime documentDate,
        UploadDocumentCategory category,
        bool overwriteExisting,
        CancellationToken cancellationToken);
}