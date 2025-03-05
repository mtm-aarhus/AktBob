using AktBob.Database.Contracts;
using AktBob.Database.Entities;
using AktBob.Email.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AktBob.Database.Repositories;
internal class TicketRepositoryExceptionDecorator : ITicketRepository
{
    private readonly ITicketRepository _inner;
    private readonly IEmailModule _email;
    private readonly ILogger<TicketRepositoryExceptionDecorator> _logger;
    private readonly string? _emailNotificationReceiver;

    public TicketRepositoryExceptionDecorator(ITicketRepository inner, IEmailModule email, ILogger<TicketRepositoryExceptionDecorator> logger, IConfiguration configuration)
    {
        _inner = inner;
        _email = email;
        _logger = logger;
        _emailNotificationReceiver = configuration.GetValue<string>("EmailNotificationReceiver");
    }

    private void LogAndNotify(string name, Exception ex)
    {
        _logger.LogError(ex, "Error in {name}", name);
        _email.Send(_emailNotificationReceiver, $"{nameof(TicketRepository)}.{name} failure", ex.Message);
    }

    public async Task<int> Add(Ticket ticket)
    {
        try
        {
            return await _inner.Add(ticket);
        }
        catch (Exception ex)
        {
            LogAndNotify(nameof(Add), ex);
            throw;
        }
    }

    public async Task<Ticket?> Get(int id)
    {
        try
        {
            return await _inner.Get(id);
        }
        catch (Exception ex)
        {
            LogAndNotify(nameof(Get), ex);
            throw;
        }
    }

    public async Task<Ticket?> GetByDeskproTicketId(int deskproTicketId)
    {
        try
        {
            return await _inner.GetByDeskproTicketId(deskproTicketId);
        }
        catch (Exception ex)
        {
            LogAndNotify(nameof(GetByDeskproTicketId), ex);
            throw;
        }
    }

    public async Task<Ticket?> GetByPodioItemId(long podioItemId)
    {
        try
        {
            return await _inner.GetByPodioItemId(podioItemId);
        }
        catch (Exception ex)
        {
            LogAndNotify(nameof(GetByPodioItemId), ex);
            throw;
        }
    }

    public async Task<int> Update(Ticket ticket)
    {
        try
        {
            return await _inner.Update(ticket);
        }
        catch (Exception ex)
        {
            LogAndNotify(nameof(Update), ex);
            throw;
        }
    }
}
