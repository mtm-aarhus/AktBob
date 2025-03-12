using AktBob.Database.Contracts;
using AktBob.Database.Entities;
using AktBob.Database.Repositories;
using Microsoft.Extensions.Logging;

namespace AktBob.Database.Decorators;
internal class TicketRepositoryExceptionDecorator : ITicketRepository
{
    private readonly ITicketRepository _inner;
    private readonly ILogger<TicketRepository> _logger;

    public TicketRepositoryExceptionDecorator(ITicketRepository inner, ILogger<TicketRepository> logger)
    {
        _inner = inner;
        _logger = logger;
    }

    public async Task<bool> Add(Ticket ticket)
    {
        try
        {
            return await _inner.Add(ticket);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {name}", nameof(Add));
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
            _logger.LogError(ex, "Error in {name}", nameof(Get));
            throw;
        }
    }

    public async Task<IEnumerable<Ticket>> GetAll(int? DeskproId, long? PodioItemId, Guid? FilArkivCaseId)
    {
        try
        {
            return await _inner.GetAll(DeskproId, PodioItemId, FilArkivCaseId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {name}", nameof(GetAll));
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
            _logger.LogError(ex, "Error in {name}", nameof(GetByDeskproTicketId));
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
            _logger.LogError(ex, "Error in {name}", nameof(GetByPodioItemId));
            throw;
        }
    }

    public async Task<bool> Update(Ticket ticket)
    {
        try
        {
            return await _inner.Update(ticket);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {name}", nameof(Update));
            throw;
        }
    }
}
