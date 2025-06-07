using System.Net;
using System.Text.Encodings.Web;
using System.Text.Json;
using Aktbob.Modules.OpenOrchestrator.Contracts;
using AktBob.Shared.Extensions;
using Ardalis.GuardClauses;
using ErrorOr;

namespace Aktbob.Modules.OpenOrchestrator;

public static class RegisterModuleClient
{
    public static IServiceCollection AddOpenOrchestratorModuleClient(this IServiceCollection services, IConfiguration configuration)
    {
        var baseAddress = Guard.Against.NullOrEmpty(configuration.GetValue<string>("Modules:OpenOrchestrator"));
        
        services.AddScoped<OpenOrchestratorModuleClient>();
        services.AddHttpClient<OpenOrchestratorModuleClient>(client =>
        {
            client.BaseAddress = new Uri(baseAddress);
        });
        
        return services;
    }
}

public class OpenOrchestratorModuleClient(HttpClient httpClient)
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly JsonSerializerOptions _jsonSerializerOptions =new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public async Task<ErrorOr<Guid>> AddQueueItem(string queueName, string reference, object? payload, CancellationToken cancellationToken = default)
    {
        try
        {
            var url = new Uri("queue-item", UriKind.Relative);
            var body = new AddQueueItemRequest(queueName, reference, JsonDocument.Parse(payload?.ToJson() ?? "{}"));
            var response = await _httpClient.PostAsJsonAsync(url, body, _jsonSerializerOptions, cancellationToken);
            var result = await response.Content.ReadAsStringAsync(cancellationToken);
            return Guid.Parse(result).ToErrorOr();
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return Error.NotFound("DownloadMessageAttachment.NotFound", "Message attachment not found");
        }
        catch (Exception ex)
        {
            return Error.Failure("DownloadMessageAttachment.Failure", $"Error downloading message attachment: {ex.Message}");
        }
        
    }
}