using System.Net.Http.Json;
using System.Text.Json;
using AktBob.Shared.Contracts.Modules.OpenOrchestrator;
using AktBob.Shared.Extensions;
using ErrorOr;

namespace AktBob.Shared.ModuleClients.OpenOrchestratorModule;

internal class OpenOrchestratorModuleClient(HttpClient httpClient) : IOpenOrchestratorModuleClient
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly JsonSerializerOptions _jsonSerializerOptions = SerializerConfiguration.SerializerOptions(caseInsensitive: false, jsonNamingPolicy: null);

    public async Task<ErrorOr<CreateQueueItemResponse>> AddQueueItem(string queueName, string reference, object? payload, CancellationToken cancellationToken = default)
    {
        try
        {
            var url = new Uri("queue-item", UriKind.Relative);
            var body = new CreateQueueItemRequest(queueName, reference, JsonDocument.Parse(payload?.ToJson(_jsonSerializerOptions) ?? "{}"));
            var response = await _httpClient.PostAsJsonAsync(url, body, SerializerConfiguration.SerializerOptions(), cancellationToken);
            var result = await response.Content.ReadFromJsonAsync<CreateQueueItemResponse>(cancellationToken);
            return result?.ToErrorOr() ?? Error.Failure("AddQueueItem.Failure", "Error creating queue item");
        }
        catch (Exception ex)
        {
            return Error.Failure("AddQueueItem.Failure", $"Error creating queue item: {ex.Message}");
        }
    }
}