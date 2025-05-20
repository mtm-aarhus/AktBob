using AktBob.CloudConvert.Models.JobResponse;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace AktBob.CloudConvert;
internal class CloudConvertClient : ICloudConvertClient
{
    private readonly HttpClient _httpClient;

    public CloudConvertClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ErrorOr<Guid>> CreateJob(object payload, CancellationToken cancellationToken = default)
    {

        var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions { PropertyNameCaseInsensitive = false });
        var stringContent = new StringContent(json, Encoding.UTF8, "application/json");

        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri("jobs", UriKind.Relative),
            Content = stringContent
        };

        var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadFromJsonAsync<JobResponseRoot>();
        if (content?.Data is not null)
        {
            return content.Data.Id;
        }

        return Error.Failure("CloudConvertClientCreateJob.Failure", "Error creating Cloud Convert job");
    }


    public async Task<ErrorOr<JobResponseRoot>> GetJob(Guid jobId, CancellationToken cancellationToken = default)
    {
        var result = await _httpClient.GetFromJsonAsync<JobResponseRoot>($"jobs/{jobId}", cancellationToken);
        if (result?.Data is not null)
        {
            return result;
        }

        return Error.Failure("CloudConvertClientGetJob.Failure", $"Error getting job {jobId}");
    }


    public async Task<ErrorOr<byte[]>> GetFile(string url, CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri(url, UriKind.Absolute)
        };

        _httpClient.DefaultRequestHeaders.Remove("Authorization");

        var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var contentDisposition = response.Content.Headers.ContentDisposition;
        var filename = contentDisposition?.FileName ?? string.Empty;

        using var stream = await response.Content.ReadAsStreamAsync();
        if (stream is null)
        {
            return Error.Failure("CloudConvertClientGetFile.Failure", "Stream is null");
        }

        using var memoryStream = new MemoryStream();
        stream.CopyTo(memoryStream);
        return memoryStream.ToArray();
    }
}