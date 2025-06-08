using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using Aktbob.Modules.OpenOrchestrator.Client.DTOs;
using AktBob.Shared;
using AktBob.Shared.Contracts.Modules.OpenOrchestrator;

namespace Aktbob.Modules.OpenOrchestrator.Client;

internal class OpenOrchestratorClient(HttpClient httpClient)
{
    private readonly HttpClient _httpClient = httpClient;

    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        WriteIndented = false,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        PropertyNameCaseInsensitive = false
    };
    
    public async Task<CreateQueueItemResponse?> PostQueueItem(string queueName, string? reference, JsonDocument? payload, CancellationToken cancellationToken = default)
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
        var result = JsonSerializer.Deserialize<CreateQueueItemResponse>(content, SerializerConfiguration.SerializerOptions());
        return result;
    }
}