using AAK.GetOrganized;
using AAK.GetOrganized.UploadDocument;
using AktBob.GetOrganized.Contracts;
using Ardalis.Result;
using Microsoft.Extensions.Configuration;

namespace AktBob.GetOrganized.Handlers;
internal class UploadDocumenHandler(IConfiguration configuration, IGetOrganizedClient getOrganizedClient) : IUploadDocumentHandler
{
    private readonly IConfiguration _configuration = configuration;
    private readonly IGetOrganizedClient _getOrganizedClient = getOrganizedClient;

    public async Task<Result<int>> Handle(UploadDocumentCommand command, CancellationToken cancellationToken)
    {
        var metadata = new UploadDocumentMetadata
        {
            CustomProperty = command.CustomProperty,
            DocumentCategory = MapDocumentCategory(command.Category),
            DocumentDate = command.DocumentDate
        };

        var listName = _configuration.GetValue<string>("GetOrganized:DefaultListName") ?? "Dokumenter";

        var result = await _getOrganizedClient.UploadDocument(command.Bytes,
                                                              command.CaseNumber,
                                                              listName,
                                                              string.Empty,
                                                              command.FileName,
                                                              metadata,
                                                              command.OverwriteExisting,
                                                              cancellationToken);

        if (result is not null)
        {
            return result.DocumentId;
        }

        return Result.Error($"Error uploading document to GetOrganized (CaseNumber: {command.CaseNumber}, FileName: '{command.FileName}')");
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
