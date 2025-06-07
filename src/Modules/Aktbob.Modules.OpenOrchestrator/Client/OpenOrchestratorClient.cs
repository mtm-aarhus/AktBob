using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using Aktbob.Modules.OpenOrchestrator.Client.DTOs;

namespace Aktbob.Modules.OpenOrchestrator.Client;

internal class OpenOrchestratorClient(HttpClient httpClient)
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public async Task<PostQueueItemResponse?> PostQueueItem(string queueName, string? reference, JsonDocument? payload, CancellationToken cancellationToken = default)
    {
        var body = new QueueItem
        {
            QueueName = queueName,
            Data = payload,
            Reference = reference ?? string.Empty
        };
        
        var json = JsonSerializer.Serialize(body, _jsonSerializerOptions);

        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            Content = new StringContent(json, Encoding.UTF8, "application/json"),
            RequestUri = new Uri($"queue", UriKind.Relative)
        };
        
        var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        var result = JsonSerializer.Deserialize<PostQueueItemResponse>(content, _jsonSerializerOptions);
        return result;
    }
}