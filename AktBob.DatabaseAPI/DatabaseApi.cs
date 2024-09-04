using AktBob.DatabaseAPI.Contracts;
using Ardalis.Result;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace AktBob.DatabaseAPI;
internal class DatabaseApi : IDatabaseApi
{
    private readonly HttpClient _httpClient;

    public DatabaseApi(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<Result<IEnumerable<TicketDto>>> GetTicketsByDeskproId(int deskproId, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync(new Uri($"Database/Tickets?deskproId={deskproId}", UriKind.Relative), cancellationToken);
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

    public async Task<Result<IEnumerable<TicketDto>>> GetTicketsByPodioItemId(long podioItemId, CancellationToken cancellationToken = default)
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
            var jsonValues = new Dictionary<string, object>();

            if (podioItemId != null)
            {
                jsonValues.Add("podioItemId", Convert.ToInt64(podioItemId));
            }

            if (filArkivCaseId != null)
            {
                jsonValues.Add("filArkivCaseId", (Guid)filArkivCaseId);
            }

            var json = JsonSerializer.Serialize(jsonValues);
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Patch,
                RequestUri = new Uri($"Database/Cases/{id}", UriKind.Relative),
                Content = new StringContent(json, encoding: Encoding.UTF8, "application/json")
            };

            var response = await _httpClient.SendAsync(request, cancellationToken);
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

    public async Task<Result<CaseDto>> PostCase(int ticketId, long? podioItemId, Guid? filArkivCaseId, CancellationToken cancellationToken = default)
    {
        try
        {
            var body = new
            {
                ticketId,
                podioItemId,
                filArkivCaseId
            };

            var json = JsonSerializer.Serialize(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"Database/Cases", UriKind.Relative),
                Content = new StringContent(json, encoding: Encoding.UTF8, "application/json")
            };

            var response = await _httpClient.SendAsync(request, cancellationToken);
            var content = await response.Content.ReadAsStringAsync();

            var @case = JsonSerializer.Deserialize<CaseDto>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

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
