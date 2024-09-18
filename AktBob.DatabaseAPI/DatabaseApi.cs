using AktBob.DatabaseAPI.Contracts.DTOs;
using Ardalis.Result;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace AktBob.DatabaseAPI;
internal class DatabaseApi : IDatabaseApi
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<DatabaseApi> _logger;

    public DatabaseApi(HttpClient httpClient, ILogger<DatabaseApi> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<TicketDto>>> GetTicketsByDeskproId(int deskproId, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync(new Uri($"Database/Tickets?deskproId={deskproId}", UriKind.Relative), cancellationToken);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var tickets = JsonSerializer.Deserialize<IEnumerable<TicketDto>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (tickets is null)
            {
                return Result.Success(Enumerable.Empty<TicketDto>());
            }

            return Result.Success(tickets);
        }
        catch (HttpRequestException e)
        {
            _logger.LogError("HttpRequestException requesting API for tickets by Deskpro ID #{deskproId}. StatusCode: {statusCode}. Error: {message}", deskproId, e.StatusCode, e.Message);
            return Result.Error();
        }
        catch (Exception e)
        {
            _logger.LogError("Error requesting API for tickets by Deskpro ID #{deskproId}. Error: {message}", deskproId, e.Message);
            return Result.Error();
        }
    }

    public async Task<Result<IEnumerable<TicketDto>>> GetTicketsByPodioItemId(long podioItemId, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync(new Uri($"Database/Tickets?podioItemId={podioItemId}", UriKind.Relative), cancellationToken);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var tickets = JsonSerializer.Deserialize<IEnumerable<TicketDto>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (tickets is null)
            {
                return Result.Success(Enumerable.Empty<TicketDto>());
            }

            return Result.Success(tickets);
        }
        catch (HttpRequestException e)
        {
            _logger.LogError("HttpRequestException requesting API for tickets by Podio Item ID #{podioItemId}. StatusCode: {statusCode}. Error: {message}", podioItemId, e.StatusCode, e.Message);
            return Result.Error();
        }
        catch (Exception e)
        {
            _logger.LogError("Error requesting API for tickets by by Podio Item ID #{podioItemId}. Error: {message}", podioItemId, e.Message);
            return Result.Error();
        }
    }

    public async Task<Result<IEnumerable<MessageDto>>> GetMessagesNotJournalized(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync(new Uri($"Database/Messages?includeJournalized=false", UriKind.Relative), cancellationToken);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var messages = JsonSerializer.Deserialize<IEnumerable<MessageDto>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (messages is null)
            {
                return Result.Success(Enumerable.Empty<MessageDto>());
            }

            return Result.Success(messages);
            
        }
        catch (HttpRequestException e)
        {
            _logger.LogError("HttpRequestException requesting API for messages not journalized. StatusCode: {statusCode}. Error: {message}", e.StatusCode, e.Message);
            return Result.Error();
        }
        catch (Exception e)
        {
            _logger.LogError("Error requesting API for messages not journalized. Error: {message}", e.Message);
            return Result.Error();
        }
    }

    public async Task<Result<MessageDto>> UpdateMessage(int id, int? goDocumentId, CancellationToken cancellationToken = default)
    {
        try
        {
            var jsonValues = new Dictionary<string, object>();

            if (goDocumentId != null)
            {
                jsonValues.Add("goDocumentId", goDocumentId);
            }

            var json = JsonSerializer.Serialize(jsonValues);
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Patch,
                RequestUri = new Uri($"Database/Messages/{id}", UriKind.Relative),
                Content = new StringContent(json, encoding: Encoding.UTF8, "application/json")
            };

            var response = await _httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var message = JsonSerializer.Deserialize<MessageDto>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (message is null)
            {
                return Result.Error();
            }

            return Result.Success(message);
        }
        catch (HttpRequestException e)
        {
            _logger.LogError("HttpRequestException requesting API for updating message #{id}. StatusCode: {statusCode}. Error: {message}", id, e.StatusCode, e.Message);
            return Result.Error();
        }
        catch (Exception e)
        {
            _logger.LogError("Error requesting API for updating message #{id}. Error: {message}", id, e.Message);
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
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var @case = JsonSerializer.Deserialize<CaseDto>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (@case is null)
            {
                return Result.Error();
            }

            return Result.Success(@case);
        }
        catch (HttpRequestException e)
        {
            _logger.LogError("HttpRequestException requesting API for updating case #{id}. StatusCode: {statusCode}. Error: {message}", id, e.StatusCode, e.Message);
            return Result.Error();
        }
        catch (Exception e)
        {
            _logger.LogError("Error requesting API for updating case #{id}. Error: {message}", id, e.Message);
            return Result.Error();
        }
    }

    public async Task<Result<CaseDto>> PostCase(int ticketId, string caseNumber, long? podioItemId, Guid? filArkivCaseId, CancellationToken cancellationToken = default)
    {
        try
        {
            var body = new
            {
                ticketId,
                caseNumber,
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
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var @case = JsonSerializer.Deserialize<CaseDto>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (@case is null)
            {
                return Result.Error();
            }

            return Result.Success(@case);
        }
        catch (HttpRequestException e)
        {
            _logger.LogError("HttpRequestException requesting API for positing case (ticketId: {ticketId}, caseNumber: {caseNumber}, FilArkivCaseId: {filArkivCaseId}). StatusCode: {statusCode}. Error: {message}", ticketId, caseNumber, filArkivCaseId.ToString(), e.StatusCode, e.Message);
            return Result.Error();
        }
        catch (Exception e)
        {
            _logger.LogError("Error requesting API for positing case (ticketId: {ticketId}, caseNumber: {caseNumber}, FilArkivCaseId: {filArkivCaseId}). Error: {message}", ticketId, caseNumber, filArkivCaseId.ToString(), e.Message);
            return Result.Error();
        }
    }
}
