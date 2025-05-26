using AAK.GetOrganized;
using AAK.GetOrganized.UploadDocument;
using AktBob.GetOrganized.Contracts;
using AktBob.GetOrganized.Handlers.UploadDocument;
using ErrorOr;
using Microsoft.Extensions.Configuration;

namespace AktBob.GetOrganized.Handlers;
internal class UploadDocumentHandler(IConfiguration configuration, IGetOrganizedClient getOrganizedClient) : IUploadDocumentHandler
{
    public async Task<ErrorOr<int>> Handle(
        byte[] bytes,
        string caseNumber,
        string fileName,
        string customProperty,
        DateTime documentDate,
        UploadDocumentCategory category,
        bool overwriteExisting,
        CancellationToken cancellationToken)
    {
        var metadata = new UploadDocumentMetadata
        {
            CustomProperty = customProperty,
            DocumentCategory = MapDocumentCategory(category),
            DocumentDate = documentDate
        };

        var listName = configuration.GetValue<string>("GetOrganized:DefaultListName") ?? "Dokumenter";

        var result = await getOrganizedClient.UploadDocument(
            bytes,
            caseNumber,
            listName,
            string.Empty,
            fileName,
            metadata,
            overwriteExisting,
            cancellationToken);

        if (result is not null)
        {
            return result.DocumentId;
        }

        return Error.Failure("GetOrganized.UploadDocumenHandlerFailure", $"Error uploading document to GetOrganized (CaseNumber: {caseNumber}, FileName: '{fileName}')");
    }

    private DocumentCategory MapDocumentCategory(UploadDocumentCategory category)
    {
        switch (category)
        {
            case UploadDocumentCategory.Internal:
            default:
                return DocumentCategory.Intern;

            case UploadDocumentCategory.Incoming:
                return DocumentCategory.Indgående;

            case UploadDocumentCategory.Outgoing:
                return DocumentCategory.Udgående;
        }
    }
}
