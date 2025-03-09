using AktBob.Database.Contracts;
using AktBob.Database.Entities;
using Microsoft.Extensions.Logging;

namespace AktBob.Database;
internal class TicketRepositoryLoggingDecorator : ITicketRepository
{
    private readonly ITicketRepository _inner;
    private readonly ILogger<TicketRepositoryLoggingDecorator> _logger;

    public TicketRepositoryLoggingDecorator(ITicketRepository inner, ILogger<TicketRepositoryLoggingDecorator> logger)
    {
        _inner = inner;
        _logger = logger;
    }

    public async Task<bool> Add(Ticket ticket)
    {
        _logger.LogInformation("Adding to database {ticket}", ticket);
        var success = await _inner.Add(ticket);

        if (!success)
        {
            _logger.LogWarning("No rows were affected when trying to add {ticket}", ticket);
        }

        return success;
    }

    public async Task<Ticket?> Get(int id)
    {
        _logger.LogInformation("Getting ticket {id}", id);
        var ticket = await _inner.Get(id);

        if (ticket is null)
        {
            _logger.LogWarning("Ticket {id} not found in database", id);
        }

        return ticket;
    }

    public async Task<IEnumerable<Ticket>> GetAll(int? DeskproId, long? PodioItemId, Guid? FilArkivCaseId)
    {
        _logger.LogInformation("Getting all tickets by DeskproId = {deskproId}, PodioItemId = {podioItemId}, FilArkivCaseId = {filArkivCaseId}", DeskproId, PodioItemId, FilArkivCaseId);
        var tickets = await _inner.GetAll(DeskproId, PodioItemId, FilArkivCaseId);

        _logger.LogInformation("{count} tickets found by DeskproId = {deskproId}, PodioItemId = {podioItemId}, FilArkivCaseId = {filArkivCaseId}", tickets.Count(), DeskproId, PodioItemId, FilArkivCaseId);
        return tickets;
    }

    public async Task<Ticket?> GetByDeskproTicketId(int deskproTicketId)
    {
        _logger.LogWarning("Getting ticket by Deskpro ticket id {id}", deskproTicketId);

        var ticket = await _inner.GetByDeskproTicketId(deskproTicketId);
    
        if (ticket is null)
        {
            _logger.LogWarning("Ticket with Deskpro ticket id {id} not found in database", deskproTicketId);
        }

        return ticket;
    }

    public async Task<Ticket?> GetByPodioItemId(long podioItemId)
    {
        _logger.LogWarning("Getting ticket by Podio item id {id}", podioItemId);

        var ticket = await _inner.GetByPodioItemId(podioItemId);

        if (ticket is null)
        {
            _logger.LogWarning("Ticket with Podio item id {id} not found in database", podioItemId);
        }

        return ticket;
    }

    public async Task<int> Update(Ticket ticket)
    {
        _logger.LogInformation("Updating {ticket}", ticket);
        
        var rowsAffected = await _inner.Update(ticket);

        if (rowsAffected == 0)
        {
            _logger.LogWarning("No rows were affected when trying to update {ticket}", ticket);
        }

        return rowsAffected;
    }
}
