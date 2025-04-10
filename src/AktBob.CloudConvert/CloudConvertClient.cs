﻿using AktBob.CloudConvert.Models.JobResponse;
using System.Net.Http.Json;
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

    public async Task<Result<Guid>> CreateJob(object payload, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Creating Cloud Convert job");

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

            var content = await response.Content.ReadFromJsonAsync<JobResponseRoot>();

            if (content?.Data is not null)
            {
                _logger.LogInformation("Cloud Convert job created: {id}", content.Data.Id);
                return content.Data.Id;
            }

            _logger.LogError("Error creating Cloud Convert job");
            return Result.Error();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating Cloud Convert job");
            return Result.Error();
        }
    }


    public async Task<Result<JobResponseRoot>> GetJob(Guid jobId, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _httpClient.GetFromJsonAsync<JobResponseRoot>($"jobs/{jobId}", cancellationToken);
            if (result?.Data is not null)
            {
                return result;
            }

            return Result.Error();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting job {id}", jobId);
            return Result.Error();
        }
    }


    public async Task<Result<byte[]>> GetFile(string url, CancellationToken cancellationToken = default)
    {
        try
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
            using var memoryStream = new MemoryStream();
            stream.CopyTo(memoryStream);
            return memoryStream.ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting file {url}", url);
            return Result.Error();
        }
    }
}