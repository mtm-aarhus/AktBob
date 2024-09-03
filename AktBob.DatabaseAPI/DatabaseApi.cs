using AktBob.DatabaseAPI.Contracts;
using Ardalis.Result;
using System.Net.Http.Json;
using System.Text.Json;

namespace AktBob.DatabaseAPI;
internal class DatabaseApi : IDatabaseApi
{
    private readonly HttpClient _httpClient;

    public DatabaseApi(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<Result<IEnumerable<TicketDto>>> GetTicketByPodioItemId(long podioItemId, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync(new Uri($"Database/Tickets?podioItemId={podioItemId}", UriKind.Relative), cancellationToken);
            var content = await response.Content.ReadAsStringAsync();

            var tickets = JsonSerializer.Deserialize<IEnumerable<TicketDto>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (tickets is null)
            {
                return Result.NotFound();
            }

            return Result.Success(tickets);
        }
        catch (Exception)
        {
            return Result.Error();
        }
    }

    public async Task<Result<CaseDto>> UpdateCase(int id, long? podioItemId, Guid? filArkivCaseId, CancellationToken cancellationToken = default)
    {
        try
        {
            var json = new
            {
                podioItemId,
                filArkivCaseId
            };

            var response = await _httpClient.PatchAsJsonAsync(new Uri($"Database/Case/{id}", UriKind.Relative), cancellationToken);
            var cotent = await response.Content.ReadAsStringAsync();

            var @case = JsonSerializer.Deserialize<CaseDto>(cotent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (@case is null)
            {
                return Result.Error();
            }

            return Result.Success(@case);
        }
        catch (Exception)
        {
            return Result.Error();
        }
    }
}
