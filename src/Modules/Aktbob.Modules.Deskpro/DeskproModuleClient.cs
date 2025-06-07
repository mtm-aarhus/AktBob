using System.Net;
using System.Text.Encodings.Web;
using System.Text.Json;
using Aktbob.Modules.Deskpro.Contracts.DTOs;
using AktBob.Shared.Types.Deskpro;
using Ardalis.GuardClauses;

namespace Aktbob.Modules.Deskpro;

public static class RegisterModuleClient
{
    public static IServiceCollection AddDeskproModuleClient(this IServiceCollection services, IConfiguration configuration)
    {
        var baseAddress = Guard.Against.NullOrEmpty(configuration.GetValue<string>("Modules:Deskpro"));
        
        services.AddScoped<DeskproModuleClient>();
        services.AddHttpClient<DeskproModuleClient>(client =>
        {
            client.BaseAddress = new Uri(baseAddress);
        });
        
        return services;
    }
}

public class DeskproModuleClient(HttpClient httpClient)
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly JsonSerializerOptions _jsonSerializerOptions =new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public async Task<ErrorOr<Stream>> DownloadMessageAttachment(string downloadUrl, CancellationToken cancellationToken = default)
    {
        try
        {
            var url = new Uri($"download-message-attachment?url={downloadUrl}", UriKind.Relative);
            var result = await _httpClient.GetStreamAsync(url, cancellationToken);
            return result.ToErrorOr();
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return Error.NotFound("DownloadMessageAttachment.NotFound", "Message attachment not found");
        }
        catch (Exception ex)
        {
            return Error.Failure("DownloadMessageAttachment.Failure", $"Error downloading message attachment: {ex.Message}");
        }
        
    }

    public async Task<ErrorOr<IReadOnlyCollection<CustomFieldSpecificationDto>>> GetCustomFieldSpecifications(CancellationToken cancellationToken = default)
    {
        try
        {
            var url = new Uri("custom-field-specifications", UriKind.Relative);
            var result = await _httpClient.GetFromJsonAsync<IReadOnlyCollection<CustomFieldSpecificationDto>>(url, cancellationToken);
            return result?.ToErrorOr() ?? Error.Failure("GetCustomFieldSpecifications.Failure", "Custom field specification value is null");
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return Error.NotFound("GetCustomFieldSpecifications.NotFound", "Custom field specification not found");
        }
        catch (Exception ex)
        {
            return Error.Failure("GetCustomFieldSpecifications.Failure", $"Error getting custom field specifications: {ex.Message}");
        }
    }
    
    public async Task<ErrorOr<MessageDto>> GetMessage(MessageId messageId, CancellationToken cancellationToken = default)
    {
        try
        {
            var url = new Uri($"tickets/{messageId.TicketId}/messages/{messageId.Id}", UriKind.Relative);
            var result = await _httpClient.GetFromJsonAsync<MessageDto>(url, cancellationToken);
            return result?.ToErrorOr() ?? Error.Failure("GetMessage.Failure", "Message value is null");
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return Error.NotFound("GetMessage.NotFound", "Message not found");
        }
        catch (Exception ex)
        {
            return Error.Failure("GetMessage.Failure", $"Error getting message: {ex.Message}");
        }
    }
    
    public async Task<ErrorOr<IReadOnlyCollection<AttachmentDto>>> GetMessageAttachments(MessageId messageId, CancellationToken cancellationToken = default)
    {
        try
        {
            var url = new Uri($"tickets/{messageId.TicketId}/messages/{messageId.Id}/attachments", UriKind.Relative);
            var result = await _httpClient.GetFromJsonAsync<IReadOnlyCollection<AttachmentDto>>(url, cancellationToken);
            return result?.ToErrorOr() ?? Error.Failure("GetMessageAttachments.Failure", "Message attachments value is null");
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return Error.NotFound("GetMessageAttachments.NotFound", "Message attachments not found");
        }
        catch (Exception ex)
        {
            return Error.Failure("GetMessageAttachments.Failure", $"Error getting message attachments: {ex.Message}");
        }
    }
    
    public async Task<ErrorOr<IReadOnlyCollection<MessageDto>>> GetMessages(TicketId ticketId, CancellationToken cancellationToken = default)
    {
        try
        {
            var url = new Uri($"tickets/{ticketId.Value}/messages", UriKind.Relative);
            var result = await _httpClient.GetFromJsonAsync<IReadOnlyCollection<MessageDto>>(url, cancellationToken);
            return result?.ToErrorOr() ?? Error.Failure("GetMessages.Failure", "Messages value is null");
        }
        catch (HttpRequestException ex)
            when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return Error.NotFound("GetMessages.NotFound", "Messages not found");
        }
        catch (Exception ex)
        {
            return Error.Failure("GetMessages.Failure", $"Error getting messages: {ex.Message}");
        }
    }
    
    public async Task<ErrorOr<PersonDto>> GetPersonByEmail(string email, CancellationToken cancellationToken = default)
    {
        try
        {
            var url = new Uri($"persons?email={email}", UriKind.Relative);
            var result = await _httpClient.GetFromJsonAsync<PersonDto>(url, cancellationToken);
            return result?.ToErrorOr() ?? Error.Failure("GetPersonByEmail.Failure", "Person value is null");
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return Error.NotFound("GetPersonByEmail.NotFound", "Person not found");
        }
        catch (Exception ex)
        {
            return Error.Failure("GetPersonByEmail.Failure", $"Error getting person: {ex.Message}");
        }
    }
    
    public async Task<ErrorOr<PersonDto>> GetPersonById(int personId, CancellationToken cancellationToken = default)
    {
        try
        {
            var url = new Uri($"persons/{personId}", UriKind.Relative);
            var result = await _httpClient.GetFromJsonAsync<PersonDto>(url, cancellationToken);
            return result?.ToErrorOr() ?? Error.Failure("GetPersonByEmail.Failure", "Person value is null");
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return Error.NotFound("GetPersonByEmail.NotFound", "Person not found");
        }
        catch (Exception ex)
        {
            return Error.Failure("GetPersonByEmail.Failure", $"Error getting person: {ex.Message}");
        }
    }
    
    public async Task<ErrorOr<TeamDto>> GetTeam(int teamId, CancellationToken cancellationToken = default)
    {
        try
        {
            var url = new Uri($"teams/{teamId}", UriKind.Relative);
            var result = await _httpClient.GetFromJsonAsync<TeamDto>(url, cancellationToken);
            return result?.ToErrorOr() ?? Error.Failure("GetTeam.Failure", "Team value is null");
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return Error.NotFound("GetTeam.NotFound", "Team not found");
        }
        catch (Exception ex)
        {
            return Error.Failure("GetTeam.Failure", $"Error getting team: {ex.Message}");
        }
    }
    
    public async Task<ErrorOr<TicketDto>> GetTicket(TicketId ticketId, CancellationToken cancellationToken = default)
    {
        try
        {
            var url = new Uri($"tickets/{ticketId.Value}", UriKind.Relative);
            var result = await _httpClient.GetFromJsonAsync<TicketDto>(url, cancellationToken);
            return result?.ToErrorOr() ?? Error.Failure("GetTicket.Failure", "Ticket value is null");
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return Error.NotFound("GetTicket.NotFound", "Ticket not found");
        }
        catch (Exception ex)
        {
            return Error.Failure("GetTicket.Failure", $"Error getting ticket: {ex.Message}");
        }
    }
    
    public async Task<ErrorOr<TicketDto>> SearchTicketsByFields(int[] fields, string searchValue, CancellationToken cancellationToken = default)
    {
        try
        {
            var fieldsCombined = string.Join(',', fields);
            var url = new Uri($"search/tickets?fields={fieldsCombined}&searchValue={searchValue}", UriKind.Relative);
            var result = await _httpClient.GetFromJsonAsync<TicketDto>(url, cancellationToken);
            return result?.ToErrorOr() ?? Error.Failure("GetTicket.Failure", "Ticket value is null");
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return Error.NotFound("GetTicket.NotFound", "Ticket not found");
        }
        catch (Exception ex)
        {
            return Error.Failure("GetTicket.Failure", $"Error getting ticket: {ex.Message}");
        }
    }
    
    public async Task<ErrorOr<Success>> InvokeWebhook(string webhookId, JsonDocument? payload, CancellationToken cancellationToken = default)
    {
        try
        {
            var url = new Uri($"webhook/{webhookId}", UriKind.Relative);
            await _httpClient.PostAsJsonAsync(url, payload, _jsonSerializerOptions, cancellationToken);
            return Result.Success;
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return Error.NotFound("GetTicket.NotFound", "Ticket not found");
        }
        catch (Exception ex)
        {
            return Error.Failure("GetTicket.Failure", $"Error getting ticket: {ex.Message}");
        }
    }
}