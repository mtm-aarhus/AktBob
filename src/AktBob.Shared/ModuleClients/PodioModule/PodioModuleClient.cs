using System.Net.Http.Json;
using System.Text.Json;
using AktBob.Shared.Contracts.Modules.Podio;
using ErrorOr;

namespace AktBob.Shared.ModuleClients.PodioModule;

internal class PodioModuleClient(HttpClient httpClient) : IPodioModuleClient
{
    private readonly HttpClient _httpClient = httpClient;

    public async Task<ErrorOr<ItemDto>> GetItem(int appId, long itemId, CancellationToken cancellationToken = default)
    {
        try
        {
            var url = new Uri($"apps/{appId}/items/{itemId}", UriKind.Relative);
            var result = await _httpClient.GetFromJsonAsync<ItemDto>(url, cancellationToken);
            return result?.ToErrorOr() ??
                   Error.Failure($"{nameof(PodioModuleClient)}.{nameof(GetItem)}", "Item value is null");
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return Error.NotFound($"{nameof(PodioModuleClient)}.{nameof(GetItem)}", $"App {appId} item {itemId} could not be found.");
        }
        catch (Exception ex)
        {
            return Error.Failure($"{nameof(PodioModuleClient)}.{nameof(GetItem)}", $"Error getting app {appId} item {itemId}: {ex.Message}");
        }
    }
    
    public async Task<ErrorOr<Success>> PostComment(int appId, long itemId, PostCommentRequest request,  CancellationToken cancellationToken = default)
    {
        try
        {
            var url = new Uri($"apps/{appId}/items/{itemId}/comment", UriKind.Relative);
            await _httpClient.PostAsJsonAsync(url, request, SerializerConfiguration.SerializerOptions(), cancellationToken);
            return Result.Success;
        }
        catch (Exception ex)
        {
            var json = JsonSerializer.Serialize(request, SerializerConfiguration.SerializerOptions());
            return Error.Failure($"{nameof(PodioModuleClient)}.{nameof(PostComment)}", $"Error posting comment on app {appId} item {itemId} with payload {json}: {ex.Message}");
        }
    }
    
    public async Task<ErrorOr<Success>> UpdateField(int appId, long itemId, UpdateFieldRequest request,  CancellationToken cancellationToken = default)
    {
        try
        {
            var url = new Uri($"apps/{appId}/items/{itemId}", UriKind.Relative);
            await _httpClient.PatchAsJsonAsync(url, request, SerializerConfiguration.SerializerOptions(), cancellationToken);
            return Result.Success;
        }
        catch (Exception ex)
        {
            var json = JsonSerializer.Serialize(request, SerializerConfiguration.SerializerOptions());
            return Error.Failure($"{nameof(PodioModuleClient)}.{nameof(UpdateField)}", $"Error updating field on app {appId} item {itemId} with payload {json}: {ex.Message}");
        }
    }
}