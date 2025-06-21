using System.Text.Json;
using AktBob.Shared.Contracts.Modules.Deskpro.DTOs;
using AktBob.Shared.Extensions;
using ErrorOr;
using Microsoft.Extensions.Logging;

namespace AktBob.Shared.ModuleClients.DeskproModule;

internal class DeskproModuleClientLogging(IDeskproModuleClient next, ILogger<DeskproModuleClient> logger) : IDeskproModuleClient
{
    public async Task<ErrorOr<Stream>> DownloadMessageAttachment(string downloadUrl, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Downloading message attachment {url}", downloadUrl);
        
        var result = await next.DownloadMessageAttachment(downloadUrl, cancellationToken);
        result.Switch(
            _ => logger.LogInformation("{url} retrieved successfully", downloadUrl),
            _ => result.LogResultErrors(logger));
        
        return result;
    }

    public async Task<ErrorOr<IReadOnlyCollection<CustomFieldSpecificationDto>>> GetCustomFieldSpecifications(CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Getting Deskpro custom field specifications");
        
        var result = await next.GetCustomFieldSpecifications(cancellationToken);
        result.Switch(
            _ => logger.LogInformation("Deskpro custom field specifications retrieved successfully"),
            _ => result.LogResultErrors(logger));
        
        return result;
    }

    public async Task<ErrorOr<MessageDto>> GetMessage(int ticketId, int messageId, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Getting Deskpro ticket {ticketId} message {messageId}", ticketId, messageId);
        
        var result = await next.GetMessage(ticketId, messageId, cancellationToken);
        result.Switch(
            value => logger.LogInformation("Deskpro message {messageId} retrieved successfully", value.Id),
            _ => result.LogResultErrors(logger));
        
        return result;
    }

    public async Task<ErrorOr<IReadOnlyCollection<AttachmentDto>>> GetMessageAttachments(int ticketId, int messageId, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Getting Deskpro message {messageId} attachments", messageId);
        
        var result = await next.GetMessageAttachments(ticketId, messageId, cancellationToken);
        result.Switch(
            value => logger.LogInformation("Deskpro message {messageId} attachments retrieved successfully (count={count})", messageId, value.Count),
            _ => result.LogResultErrors(logger));
        
        return result;
    }

    public async Task<ErrorOr<IReadOnlyCollection<MessageDto>>> GetMessages(int ticketId, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Getting Deskpro ticket {ticketId} messages", ticketId);
        
        var result = await next.GetMessages(ticketId, cancellationToken);
        result.Switch(
            value => logger.LogInformation("Deskpro ticket {ticketId} messages retrieved successfully (count={count})", ticketId, value.Count),
            _ => result.LogResultErrors(logger));
        
        return result;
    }

    public async Task<ErrorOr<PersonDto>> GetPersonByEmail(string email, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Getting Deskpro person by email {email}", email);
        
        var result = await next.GetPersonByEmail(email, cancellationToken);
        result.Switch(
            value => logger.LogInformation("Deskpro person by email {email} retrieved successfully (personId={id})", email, value.Id),
            _ => result.LogResultErrors(logger));
        
        return result;
    }

    public async Task<ErrorOr<PersonDto>> GetPersonById(int personId, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Getting Deskpro person {personId}", personId);
        
        var result = await next.GetPersonById(personId, cancellationToken);
        result.Switch(
            value => logger.LogInformation("Deskpro person {personId} retrieved successfully", value.Id),
            _ => result.LogResultErrors(logger));
        
        return result;
    }

    public async Task<ErrorOr<TeamDto>> GetTeam(int teamId, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Getting Deskpro team {teamId}", teamId);
        
        var result = await next.GetTeam(teamId, cancellationToken);
        result.Switch(
            value => logger.LogInformation("Deskpro team {teamId} retrieved successfully (name={name})", teamId, value.Name),
            _ => result.LogResultErrors(logger));
        
        return result;
    }

    public async Task<ErrorOr<TicketDto>> GetTicket(int ticketId, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Getting Deskpro ticket {ticketId}", ticketId);
        
        var result = await next.GetTicket(ticketId, cancellationToken);
        result.Switch(
            value => logger.LogInformation("Deskpro ticket {ticketId} retrieved successfully", value.Id),
            _ => result.LogResultErrors(logger));
        
        return result;
    }

    public async Task<ErrorOr<IReadOnlyCollection<TicketDto>>> SearchTicketsByFields(int[] fields, string searchValue, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Searching Deskpro tickets by fields {fields} with search value {searchValue}", string.Join(",", fields), searchValue);
        
        var result = await next.SearchTicketsByFields(fields, searchValue, cancellationToken);
        result.Switch(
            value => logger.LogInformation("Deskpro tickets searched successfully, found tickets {id}", string.Join(",", value.Select(x => x.Id))),
            _ => result.LogResultErrors(logger));
        
        return result;
    }

    public async Task<ErrorOr<Success>> InvokeWebhook(string webhookId, JsonDocument? payload, CancellationToken cancellationToken = default)
    {
        var body = JsonSerializer.Serialize(payload, SerializerConfiguration.SerializerOptions());
        logger.LogInformation("Invoking Deskpro webhook {webhookId} payload {body}", webhookId, body);
        
        var result = await next.InvokeWebhook(webhookId, payload, cancellationToken);
        result.Switch(
            _ => logger.LogInformation("Deskpro webhook {webhookId} invoked successfully", webhookId),
            _ => result.LogResultErrors(logger));
        
        return result;
    }
}