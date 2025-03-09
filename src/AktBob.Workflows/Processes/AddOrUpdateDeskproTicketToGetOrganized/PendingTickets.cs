using System.Collections.Concurrent;

namespace AktBob.Workflows.Processes.AddOrUpdateDeskproTicketToGetOrganized;
internal class PendingTicket
{
    public int TicketId { get; init; }
    public DateTime SubmittedAt { get; init; }

    public PendingTicket(int ticketId, DateTime submittedAt)
    {
        TicketId = ticketId;
        SubmittedAt = submittedAt;
    }
}

sealed internal class PendingsTickets
{
    private static readonly Lazy<PendingsTickets> _instance = new(() => new());
    public static PendingsTickets Instance => _instance.Value;

    private readonly ConcurrentDictionary<Guid, PendingTicket> _tickets = new();

    public void AddPendingTicket(PendingTicket pendingTicket)
    {
        _tickets.TryAdd(Guid.NewGuid(), pendingTicket);
    }

    public void RemovePendingTicket(PendingTicket pendingTicket)
    {
        var kvp = _tickets.FirstOrDefault(x => x.Value.TicketId == pendingTicket.TicketId && x.Value.SubmittedAt == pendingTicket.SubmittedAt);
        _tickets.TryRemove(kvp);
    }

    public bool IsMostRecent(PendingTicket ticket)
    {
        var isMostRecentTicket = !_tickets.Any(x => x.Value.TicketId == ticket.TicketId && x.Value.SubmittedAt > ticket.SubmittedAt);
        return isMostRecentTicket;
    }
}
