using Ardalis.GuardClauses;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace AktBob.UiPath;

internal class UiPathOrchestratorApi : IUiPathOrchestratorApi
{
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private const string UIPATH_TOKEN = "UIPATH_TOKEN";

    public UiPathOrchestratorApi(IConfiguration configuration, HttpClient httpClient, IMemoryCache cache)
    {
        _configuration = configuration;
        _httpClient = httpClient;
        _cache = cache;
    }

    public async Task AddQueueItem(string queueName, string reference, string queueItem)
    {
        using (var requestMessage = new HttpRequestMessage())
        {
            if (!_cache.TryGetValue(UIPATH_TOKEN, out string? token))
            {
                token = await GetToken();
            }

            var queueContent = JsonSerializer.Deserialize<object>(queueItem, new JsonSerializerOptions { Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping });
            var tenancyName = _configuration.GetValue<string>("UiPath:TenancyName");
            var body = new
            {
                itemData = new
                {
                    Name = queueName,
                    Priority = "Low",
                    Reference = reference,
                    SpecificContent = queueContent
                }
            };

            var json = JsonSerializer.Serialize(body, new JsonSerializerOptions { Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping });
            var stringContent = new StringContent(json, Encoding.UTF8, "application/json");

            requestMessage.Method = HttpMethod.Post;
            requestMessage.RequestUri = new Uri("odata/Queues/UiPathODataSvc.AddQueueItem", UriKind.Relative);
            requestMessage.Content = stringContent;
            requestMessage.Headers.Add("X-UIPATH-OrganizationUnitId", _configuration.GetValue<string>($"UiPath:{tenancyName}:OrganizationUnitId"));
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(requestMessage);
            response.EnsureSuccessStatusCode();
        };
    }


    private async Task<string> GetToken()
    {
        var tenancyName = _configuration.GetValue<string>("UiPath:TenancyName");

        var body = new
        {
            TenancyName = tenancyName,
            UsernameOrEmailAddress = _configuration.GetValue<string>($"UiPath:{tenancyName}:Username"),
            Password = _configuration.GetValue<string>($"UiPath:{tenancyName}:Password")
        };

        var json = JsonSerializer.Serialize(body);
        var stringContent = new StringContent(json, Encoding.UTF8, "application/json");

        var requestMessage = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri("api/account/authenticate", UriKind.Relative),
            Content = stringContent
        };

        var responseMessage = await _httpClient.SendAsync(requestMessage);
        responseMessage.EnsureSuccessStatusCode();

        var response = await responseMessage.Content.ReadAsStringAsync();
        var tokenString = JsonSerializer.Deserialize<OrchestratorTokenResponse>(response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Guard.Against.Null(tokenString);
        Guard.Against.NullOrEmpty(tokenString.Result);

        _cache.Set(UIPATH_TOKEN, tokenString.Result, TimeSpan.FromMinutes(25));
        return tokenString.Result;
    }


    private class OrchestratorTokenResponse
    {
        public string Result { get; set; } = string.Empty;
    }
}

