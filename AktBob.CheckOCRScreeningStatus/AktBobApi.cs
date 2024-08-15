using AktBob.CheckOCRScreeningStatus.DTOs;
using Ardalis.Result;
using System.Net.Http.Json;
using System.Text.Json;

namespace AktBob.CheckOCRScreeningStatus;
internal class AktBobApi : IAktBobApi
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
    public AktBobApi(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<Result> UpdatePodioItemFilArkivField(long podioItemId, Guid filArkivCaseId, CancellationToken cancellationToken = default)
    {
        try
        {
            var body = new
            {
                value = filArkivCaseId
            };

            var response = await _httpClient.PutAsJsonAsync(new Uri($"Podio/{podioItemId}/FilArkivField", UriKind.Relative), body, _jsonSerializerOptions, cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Error(ex.Message);
        }
    }

    public async Task<Result<PodioItemDto>> GetPodioItem(int podioAppId, long podioItemId, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<PodioItemDto>(new Uri($"Podio/{podioAppId}/{podioItemId}", UriKind.Relative), _jsonSerializerOptions, cancellationToken);

            if (response is null)
            {
                return Result.NotFound();
            }

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            return Result.Error(ex.Message);
        }
    }
}
