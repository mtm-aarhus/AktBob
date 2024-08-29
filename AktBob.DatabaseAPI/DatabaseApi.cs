using AktBob.DatabaseAPI.Contracts;
using Ardalis.Result;
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
        var response = await _httpClient.GetAsync(new Uri($"/Database/Tickets?podioItemId={podioItemId}", UriKind.Relative), cancellationToken);
        var content = await response.Content.ReadAsStringAsync();

        var tickets = JsonSerializer.Deserialize<IEnumerable<TicketDto>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (tickets is null)
        {
            return Result.NotFound();
        }

        return Result.Success(tickets);
    }
}
