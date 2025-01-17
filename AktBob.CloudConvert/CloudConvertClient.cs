using AktBob.CloudConvert.Models;
using Ardalis.Result;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace AktBob.CloudConvert;
internal class CloudConvertClient : ICloudConvertClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CloudConvertClient> _logger;

    public CloudConvertClient(HttpClient httpClient, ILogger<CloudConvertClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<Result<Guid>> CreateJob(object payload, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Creating Cloud Convert job ...");

            var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions {  PropertyNameCaseInsensitive = false });
            var stringContent = new StringContent(json, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri("jobs", UriKind.Relative),
                Content = stringContent
            };

            var response = await _httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<PostJobResponse>(content);

            if (data?.Data is not null)
            {
                _logger.LogInformation("Cloud Convert job created: '{id}'", data.Data.Id);
                return data.Data.Id;
            }

            _logger.LogError("Error creating Cloud Convert job");
            return Result.Error();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            return Result.Error();
        }
    }
}
