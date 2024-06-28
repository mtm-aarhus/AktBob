using Ardalis.Result;
using System.Net.Http.Json;

namespace AktBob.CheckOCRScreeningStatus;
internal class AktBobApi : IAktBobApi
{
    private readonly HttpClient _httpClient;

    public AktBobApi(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<Result> UpdatePodioItemFilArkivField(long podioItemId, Guid filArkivCaseId)
    {
        try
        {
            var body = new
            {
                value = filArkivCaseId
            };

            var response = await _httpClient.PutAsJsonAsync(new Uri($"Podio/{podioItemId}/FilArkivField", UriKind.Relative), body);
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Error(ex.Message);
        }
    }
}
