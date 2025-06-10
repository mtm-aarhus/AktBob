using AktBob.Database.Contracts;
using AktBob.Database.Entities;
using AktBob.Database.Repositories;
using Microsoft.Extensions.Logging;

namespace AktBob.Database.Decorators;
internal class TicketRepositoryLoggingDecorator : ITicketRepository
{
    private readonly ITicketRepository _inner;
    private readonly ILogger<TicketRepository> _logger;

    public TicketRepositoryLoggingDecorator(ITicketRepository inner, ILogger<TicketRepository> logger)
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
            _logger.LogDebug("{name}: No rows were affected when trying to add {ticket}", nameof(Add), ticket);
        }

        return success;
    }

    public async Task<Ticket?> Get(int id)
    {
        _logger.LogInformation("Getting database ticket {id}", id);
        var ticket = await _inner.Get(id);

        if (ticket is null)
        {
            _logger.LogDebug("Ticket {id} not found in database", id);
        }

        return ticket;
    }

    public async Task<IReadOnlyCollection<Ticket>> GetAll(int? deskproId, long? podioItemId, Guid? filArkivCaseId)
    {
        _logger.LogInformation("Getting all database tickets by DeskproId = {deskproId}, PodioItemId = {podioItemId}, FilArkivCaseId = {filArkivCaseId}", deskproId, podioItemId, filArkivCaseId);
        var tickets = await _inner.GetAll(deskproId, podioItemId, filArkivCaseId);
        
        if (!tickets.Any())
        {
            switch (deskproId is null)
            {
                case true when podioItemId is null && filArkivCaseId is null:
                    _logger.LogDebug("{name}: No database tickets found", nameof(GetAll));
                    break;

                case false when podioItemId is null && filArkivCaseId is null:
                    _logger.LogDebug("{name}: no database tickets found by DeskproId = {deskproId}", nameof(GetAll), deskproId);
                    break;

                case true when podioItemId is not null && filArkivCaseId is null:
                    _logger.LogDebug("{name}: no database tickets found by PodioItemId = {podioItemId}", nameof(GetAll), podioItemId);
                    break;

                case true when podioItemId is null && filArkivCaseId is not null:
                    _logger.LogDebug("{name}: no database tickets found by FilArkivCaseId = {filArkivCaseId}", nameof(GetAll), filArkivCaseId);
                    break;

                case false when podioItemId is not null && filArkivCaseId is null:
                    _logger.LogDebug("{name}: no database tickets found by DeskproId = {deskproId} AND PodioItemId = {podioItemId}", nameof(GetAll), deskproId, podioItemId);
                    break;

                case false when podioItemId is not null && filArkivCaseId is not null:
                    _logger.LogDebug("{name}: no database tickets found by DeskproId = {deskproId} AND PodioItemId = {podioItemId} AND FilArkivCaseId = {filArkivCaseId}", nameof(GetAll), deskproId, podioItemId, filArkivCaseId);
                    break;

                case true when podioItemId is not null && filArkivCaseId is not null:
                    _logger.LogDebug("{name}: no database tickets found by PodioItemId = {podioItemId} AND FilArkivCaseId = {filArkivCaseId}", nameof(GetAll), podioItemId, filArkivCaseId);
                    break;

                case false when podioItemId is null && filArkivCaseId is not null:
                    _logger.LogDebug("{name}: no database tickets found by DeskproId = {deskproId} AND FilArkivCaseId = {filArkivCaseId}", nameof(GetAll), deskproId, filArkivCaseId);
                    break;
            }
        }

        return tickets;
    }

    public async Task<Ticket?> GetByDeskproTicketId(int deskproTicketId)
    {
        _logger.LogInformation("Getting database ticket by Deskpro ticket id {id}", deskproTicketId);

        var ticket = await _inner.GetByDeskproTicketId(deskproTicketId);
    
        if (ticket is null)
        {
            _logger.LogDebug("{name}: Database ticket with Deskpro ticket id {id} not found", nameof(GetByDeskproTicketId), deskproTicketId);
        }

        return ticket;
    }

    public async Task<Ticket?> GetByPodioItemId(long podioItemId)
    {
        _logger.LogInformation("Getting ticket by Podio item id {id}", podioItemId);

        var ticket = await _inner.GetByPodioItemId(podioItemId);

        if (ticket is null)
        {
            _logger.LogDebug("{name}: Database ticket with Podio item id {id} not found", nameof(GetByPodioItemId), podioItemId);
        }

        return ticket;
    }

    public async Task<bool> Update(Ticket ticket)
    {
        _logger.LogInformation("Updating database ticket {ticket}", ticket);
        
        var success = await _inner.Update(ticket);

        if (!success)
        {
            _logger.LogDebug("{name}: No rows were affected when trying to update {ticket}", nameof(Update), ticket);
        }

        return success;
    }
}
