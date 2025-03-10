namespace AktBob.Deskpro.Decorators;

internal class ModuleExceptionDecorator(IDeskproModule inner, ILogger<DeskproModule> logger) : IDeskproModule
{
    private readonly IDeskproModule _inner = inner;
    private readonly ILogger<DeskproModule> _logger = logger;

    public async Task<Result<IEnumerable<CustomFieldSpecificationDto>>> GetCustomFieldSpecifications(CancellationToken cancellationToken)
    {
        try
        {
            return await _inner.GetCustomFieldSpecifications(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {name}", nameof(GetCustomFieldSpecifications));
            throw;
        }
    }

    public async Task<Result<MessageDto>> GetMessage(int ticketId, int messageId, CancellationToken cancellationToken)
    {
        try
        {
            return await _inner.GetMessage(ticketId, messageId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {name}", nameof(GetMessage));
            throw;
        }
    }

    public async Task<Result<Stream>> GetMessageAttachment(string downloadUrl, CancellationToken cancellationToken)
    {
        try
        {
            return await _inner.GetMessageAttachment(downloadUrl, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {name}", nameof(GetMessageAttachment));
            throw;
        }
    }

    public async Task<Result<IEnumerable<AttachmentDto>>> GetMessageAttachments(int ticketId, int messageId, CancellationToken cancellationToken)
    {
        try
        {
            return await _inner.GetMessageAttachments(ticketId, messageId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {name}", nameof(GetMessageAttachments));
            throw;
        }
    }

    public async Task<Result<IEnumerable<MessageDto>>> GetMessages(int ticketId, CancellationToken cancellationToken)
    {
        try
        {
            return await _inner.GetMessages(ticketId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {name}", nameof(GetMessages));
            throw;
        }
    }

    public async Task<Result<PersonDto>> GetPerson(int personId, CancellationToken cancellationToken)
    {
        try
        {
            return await _inner.GetPerson(personId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {name}", nameof(GetPerson));
            throw;
        }
    }

    public async Task<Result<TicketDto>> GetTicket(int ticketId, CancellationToken cancellationToken)
    {
        try
        {
            return await _inner.GetTicket(ticketId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {name}", nameof(GetTicket));
            throw;
        }
    }

    public async Task<Result<IEnumerable<TicketDto>>> GetTicketsByFieldSearch(int[] fields, string searchValue, CancellationToken cancellationToken)
    {
        try
        {
            return await _inner.GetTicketsByFieldSearch(fields, searchValue, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {name}", nameof(GetTicketsByFieldSearch));
            throw;
        }
    }

    public void InvokeWebhook(string webhookId, string payload)
    {
        try
        {
            _inner.InvokeWebhook(webhookId, payload);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {name}", nameof(InvokeWebhook));
            throw;
        }
    }
}
