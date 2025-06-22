using System.Net;
using System.Net.Http.Json;
using AktBob.Shared.Contracts.Modules.FilArkiv.Contracts.DTOs;
using ErrorOr;

namespace AktBob.Shared.ModuleClients.FilArkiv;

internal class FilArkivModuleClient(HttpClient httpClient) : IFilArkivModuleClient
{
    public async Task<ErrorOr<IReadOnlyCollection<DocumentDto>>> GetDocumentsByCaseId(Guid caseId, CancellationToken cancellationToken = default)
    {
        try
        {
            var url = new Uri($"/cases/{caseId}/documents", UriKind.Relative);
            var result = await httpClient.GetFromJsonAsync<IReadOnlyCollection<DocumentDto>>(url, cancellationToken);
            return result?.ToErrorOr() ??
                   Error.Failure($"{nameof(FilArkivModuleClient)}.{nameof(GetDocumentsByCaseId)}",
                       "Collection of DocumentDto is null");
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return Error.NotFound($"{nameof(FilArkivModuleClient)}.{nameof(GetDocumentsByCaseId)}",
                $"Documents for case {caseId} not found");
        }
        catch (Exception ex)
        {
            return Error.Failure($"{nameof(FilArkivModuleClient)}.{nameof(GetDocumentsByCaseId)}",
                $"Error getting documents for caes {caseId}: {ex.Message}");
        }

    }

    public async Task<ErrorOr<FileProcessStatusDto>> GetFileProcessStatus(Guid fileId, CancellationToken cancellationToken = default)
    {
        try
        {
            var url = new Uri($"/files/{fileId}/processing-status", UriKind.Relative);
            var result = await httpClient.GetFromJsonAsync<FileProcessStatusDto>(url, cancellationToken);
            return result?.ToErrorOr() ?? Error.Failure($"{nameof(FilArkivModuleClient)}.{nameof(GetFileProcessStatus)}", "File processing responsse is null");
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return Error.NotFound($"{nameof(FilArkivModuleClient)}.{nameof(GetFileProcessStatus)}", $"File processing status for file {fileId} not found");
        }
        catch (Exception ex)
        {
            return Error.Failure($"{nameof(FilArkivModuleClient)}.{nameof(GetFileProcessStatus)}", $"Error getting file processing status for file {fileId}: {ex.Message}");
        }
    }
}