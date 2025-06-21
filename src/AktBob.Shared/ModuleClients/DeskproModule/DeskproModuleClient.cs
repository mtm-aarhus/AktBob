using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AktBob.Shared.Contracts.Modules.Deskpro.DTOs;
using ErrorOr;

namespace AktBob.Shared.ModuleClients.DeskproModule;

internal class DeskproModuleClient(HttpClient httpClient) : IDeskproModuleClient
{
    private readonly HttpClient _httpClient = httpClient;

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
            return Error.NotFound($"{nameof(DeskproModuleClient)}.{nameof(DownloadMessageAttachment)}", $"Message attachment not found at {downloadUrl}");
        }
        catch (Exception ex)
        {
            return Error.Failure($"{nameof(DeskproModuleClient)}.{nameof(DownloadMessageAttachment)}", $"Error downloading message attachment at {downloadUrl}: {ex.Message}");
        }
        
    }

    public async Task<ErrorOr<IReadOnlyCollection<CustomFieldSpecificationDto>>> GetCustomFieldSpecifications(CancellationToken cancellationToken = default)
    {
        try
        {
            var url = new Uri("custom-field-specifications", UriKind.Relative);
            var result = await _httpClient.GetFromJsonAsync<IReadOnlyCollection<CustomFieldSpecificationDto>>(url, cancellationToken);
            return result?.ToErrorOr() ?? Error.Failure($"{nameof(DeskproModuleClient)}.{nameof(GetCustomFieldSpecifications)}", "Custom field specification value is null");
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return Error.NotFound($"{nameof(DeskproModuleClient)}.{nameof(GetCustomFieldSpecifications)}", "Custom field specification not found");
        }
        catch (Exception ex)
        {
            return Error.Failure($"{nameof(DeskproModuleClient)}.{nameof(GetCustomFieldSpecifications)}", $"Error getting custom field specifications: {ex.Message}");
        }
    }
    
    public async Task<ErrorOr<MessageDto>> GetMessage(int ticketId, int messageId, CancellationToken cancellationToken = default)
    {
        try
        {
            var url = new Uri($"tickets/{ticketId}/messages/{messageId}", UriKind.Relative);
            var result = await _httpClient.GetFromJsonAsync<MessageDto>(url, cancellationToken);
            return result?.ToErrorOr() ?? Error.Failure($"{nameof(DeskproModuleClient)}.{nameof(GetMessage)}", "Message value is null");
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return Error.NotFound($"{nameof(DeskproModuleClient)}.{nameof(GetMessage)}", $"Message {messageId} not found");
        }
        catch (Exception ex)
        {
            return Error.Failure($"{nameof(DeskproModuleClient)}.{nameof(GetMessage)}", $"Error getting message {messageId}: {ex.Message}");
        }
    }
    
    public async Task<ErrorOr<IReadOnlyCollection<AttachmentDto>>> GetMessageAttachments(int ticketId, int messageId, CancellationToken cancellationToken = default)
    {
        try
        {
            var url = new Uri($"tickets/{ticketId}/messages/{messageId}/attachments", UriKind.Relative);
            var result = await _httpClient.GetFromJsonAsync<IReadOnlyCollection<AttachmentDto>>(url, cancellationToken);
            return result?.ToErrorOr() ?? Error.Failure($"{nameof(DeskproModuleClient)}.{nameof(GetMessageAttachments)}", "Message attachments value is null");
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return Error.NotFound($"{nameof(DeskproModuleClient)}.{nameof(GetMessageAttachments)}", $"Message {messageId} attachments not found");
        }
        catch (Exception ex)
        {
            return Error.Failure($"{nameof(DeskproModuleClient)}.{nameof(GetMessageAttachments)}", $"Error getting message {messageId} attachments: {ex.Message}");
        }
    }
    
    public async Task<ErrorOr<IReadOnlyCollection<MessageDto>>> GetMessages(int ticketId, CancellationToken cancellationToken = default)
    {
        try
        {
            var url = new Uri($"tickets/{ticketId}/messages", UriKind.Relative);
            var result = await _httpClient.GetFromJsonAsync<IReadOnlyCollection<MessageDto>>(url, cancellationToken);
            return result?.ToErrorOr() ?? Error.Failure($"{nameof(DeskproModuleClient)}.{nameof(GetMessages)}", $"Ticket {ticketId}: Messages value is null");
        }
        catch (HttpRequestException ex)
            when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return Error.NotFound($"{nameof(DeskproModuleClient)}.{nameof(GetMessages)}", $"Ticket {ticketId}: Messages not found");
        }
        catch (Exception ex)
        {
            return Error.Failure($"{nameof(DeskproModuleClient)}.{nameof(GetMessages)}", $"Ticket {ticketId}: Error getting messages: {ex.Message}");
        }
    }
    
    public async Task<ErrorOr<PersonDto>> GetPersonByEmail(string email, CancellationToken cancellationToken = default)
    {
        try
        {
            var url = new Uri($"persons?email={email}", UriKind.Relative);
            var result = await _httpClient.GetFromJsonAsync<PersonDto>(url, cancellationToken);
            return result?.ToErrorOr() ?? Error.Failure($"{nameof(DeskproModuleClient)}.{nameof(GetPersonByEmail)}", $"Person value is null (email: {email})");
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return Error.NotFound($"{nameof(DeskproModuleClient)}.{nameof(GetPersonByEmail)}", $"Person not found by email {email}");
        }
        catch (Exception ex)
        {
            return Error.Failure($"{nameof(DeskproModuleClient)}.{nameof(GetPersonByEmail)}", $"Error getting person by email {email}: {ex.Message}");
        }
    }
    
    public async Task<ErrorOr<PersonDto>> GetPersonById(int personId, CancellationToken cancellationToken = default)
    {
        try
        {
            var url = new Uri($"persons/{personId}", UriKind.Relative);
            var result = await _httpClient.GetFromJsonAsync<PersonDto>(url, cancellationToken);
            return result?.ToErrorOr() ?? Error.Failure($"{nameof(DeskproModuleClient)}.{nameof(GetPersonById)}", $"Person {personId} value is null");
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return Error.NotFound($"{nameof(DeskproModuleClient)}.{nameof(GetPersonById)}", $"Person {personId} not found");
        }
        catch (Exception ex)
        {
            return Error.Failure($"{nameof(DeskproModuleClient)}.{nameof(GetPersonById)}", $"Error getting person {personId}: {ex.Message}");
        }
    }
    
    public async Task<ErrorOr<TeamDto>> GetTeam(int teamId, CancellationToken cancellationToken = default)
    {
        try
        {
            var url = new Uri($"teams/{teamId}", UriKind.Relative);
            var result = await _httpClient.GetFromJsonAsync<TeamDto>(url, cancellationToken);
            return result?.ToErrorOr() ?? Error.Failure($"{nameof(DeskproModuleClient)}.{nameof(GetTeam)}", $"Team {teamId}: value is null");
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return Error.NotFound($"{nameof(DeskproModuleClient)}.{nameof(GetTeam)}", $"Team {teamId} not found");
        }
        catch (Exception ex)
        {
            return Error.Failure($"{nameof(DeskproModuleClient)}.{nameof(GetTeam)}", $"Error getting team {teamId}: {ex.Message}");
        }
    }
    
    public async Task<ErrorOr<TicketDto>> GetTicket(int ticketId, CancellationToken cancellationToken = default)
    {
        try
        {
            var url = new Uri($"tickets/{ticketId}", UriKind.Relative);
            var result = await _httpClient.GetFromJsonAsync<TicketDto>(url, cancellationToken);
            return result?.ToErrorOr() ?? Error.Failure($"{nameof(DeskproModuleClient)}.{nameof(GetTicket)}", $"Ticket {ticketId}: value is null");
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return Error.NotFound($"{nameof(DeskproModuleClient)}.{nameof(GetTicket)}", $"Ticket {ticketId} not found");
        }
        catch (Exception ex)
        {
            return Error.Failure($"{nameof(DeskproModuleClient)}.{nameof(GetTicket)}", $"Error getting ticket {ticketId}: {ex.Message}");
        }
    }
    
    public async Task<ErrorOr<IReadOnlyCollection<TicketDto>>> SearchTicketsByFields(int[] fields, string searchValue, CancellationToken cancellationToken = default)
    {
        try
        {
            var fieldsCombined = string.Join(',', fields);
            var url = new Uri($"search/tickets?fields={fieldsCombined}&searchValue={searchValue}", UriKind.Relative);
            var result = await _httpClient.GetFromJsonAsync<IReadOnlyCollection<TicketDto>>(url, cancellationToken);
            return result?.ToErrorOr() ?? Error.Failure($"{nameof(DeskproModuleClient)}.{nameof(SearchTicketsByFields)}", "Search tickets: response value is null");
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return Error.NotFound($"{nameof(DeskproModuleClient)}.{nameof(SearchTicketsByFields)}", $"Search tickets: no tickets found searching fields {string.Join(',', fields)} with searchValue {searchValue}");
        }
        catch (Exception ex)
        {
            return Error.Failure($"{nameof(DeskproModuleClient)}.{nameof(SearchTicketsByFields)}", $"Search tickets: {ex.Message}");
        }
    }
    
    public async Task<ErrorOr<Success>> InvokeWebhook(string webhookId, JsonDocument? payload, CancellationToken cancellationToken = default)
    {
        try
        {
            var url = new Uri($"webhook/{webhookId}", UriKind.Relative);
            await _httpClient.PostAsJsonAsync(url, payload, SerializerConfiguration.SerializerOptions(), cancellationToken);
            return Result.Success;
        }
        catch (Exception ex)
        {
            var json = JsonSerializer.Serialize(payload, SerializerConfiguration.SerializerOptions());
            return Error.Failure($"{nameof(DeskproModuleClient)}.{nameof(InvokeWebhook)}", $"Error invoking webhook {webhookId} payload {json} {ex.Message}");
        }
    }
}