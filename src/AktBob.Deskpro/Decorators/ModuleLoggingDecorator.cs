using AktBob.Shared.Extensions;

namespace AktBob.Deskpro.Decorators;

internal class ModuleLoggingDecorator(IDeskproModule inner, ILogger<DeskproModule> logger) : IDeskproModule
{
    private readonly IDeskproModule _inner = inner;
    private readonly ILogger<DeskproModule> _logger = logger;

    public async Task<Result<IEnumerable<CustomFieldSpecificationDto>>> GetCustomFieldSpecifications(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting Deskpro custom field specifications ...");

        var result = await _inner.GetCustomFieldSpecifications(cancellationToken);
        if (!result.IsSuccess)
        {
            _logger.LogWarning(result.Errors.AsString());
        }

        return result;
    }

    public async Task<Result<MessageDto>> GetMessage(int ticketId, int messageId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting Deskpro ticket {ticketId} message {messageId}", ticketId, messageId);

        var result = await _inner.GetMessage(ticketId, messageId, cancellationToken);
        if (!result.IsSuccess)
        {
            _logger.LogWarning(result.Errors.AsString());
        }

        return result;
    }

    public async Task<Result<Stream>> GetMessageAttachment(string downloadUrl, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Downloading Deskpro message attachment. Url = {url}", downloadUrl);

        var result = await _inner.GetMessageAttachment(downloadUrl, cancellationToken);
        if (!result.IsSuccess)
        {
            _logger.LogWarning(result.Errors.AsString());
        }

        return result;
    }

    public async Task<Result<IEnumerable<AttachmentDto>>> GetMessageAttachments(int ticketId, int messageId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting Deskpro ticket {ticketId} message {messageId} attachments", ticketId, messageId);

        var result = await _inner.GetMessageAttachments(ticketId, messageId, cancellationToken);
        if (!result.IsSuccess)
        {
            _logger.LogWarning(result.Errors.AsString());
        }

        return result;
    }

    public async Task<Result<IEnumerable<MessageDto>>> GetMessages(int ticketId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting Deskpro ticket {id} messages", ticketId);

        var result = await _inner.GetMessages(ticketId, cancellationToken);
        if (!result.IsSuccess)
        {
            _logger.LogWarning(result.Errors.AsString());
        }

        return result;
    }

    public async Task<Result<PersonDto>> GetPerson(int personId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting Deskpro person {id}", personId);

        var result = await _inner.GetPerson(personId, cancellationToken);
        if (!result.IsSuccess)
        {
            _logger.LogWarning(result.Errors.AsString());
        }

        return result;
    }

    public async Task<Result<PersonDto>> GetPerson(string email, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting Deskpro person by email {email}", email);

        var result = await _inner.GetPerson(email, cancellationToken);
        if (!result.IsSuccess)
        {
            _logger.LogWarning(result.Errors.AsString());
        }

        return result;
    }

    public async Task<Result<TicketDto>> GetTicket(int ticketId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting Deskpro ticket {id}", ticketId);

        var result = await _inner.GetTicket(ticketId, cancellationToken);
        if (!result.IsSuccess)
        {
            _logger.LogWarning(result.Errors.AsString());
        }

        return result;
    }

    public async Task<Result<IEnumerable<TicketDto>>> GetTicketsByFieldSearch(int[] fields, string searchValue, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting Deskpro tickets by searching fields {fields} with search value = {searchValue}", fields, searchValue);

        var result = await _inner.GetTicketsByFieldSearch(fields, searchValue, cancellationToken);
        if (!result.IsSuccess)
        {
            _logger.LogWarning(result.Errors.AsString());
        }

        return result;
    }

    public void InvokeWebhook(string webhookId, string payload)
    {
        _logger.LogInformation("Enqueuing job: Invoke Deskpro inbound webhook {id} with payload {payload}", webhookId, payload);
        _inner.InvokeWebhook(webhookId, payload);
    }
}
