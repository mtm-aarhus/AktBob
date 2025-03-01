using AktBob.Database.Contracts;

namespace AktBob.JobHandlers.Handlers.CreateGetOrganizedCase;
internal class UpdateDatabase(ILogger<UpdateDatabase> logger,IServiceScopeFactory serviceScopeFactory)
{
    private readonly ILogger<UpdateDatabase> _logger = logger;
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;

    public async Task SetGetOrganizedCaseId(int deskproId, string caseId, string caseUrl, CancellationToken cancellationToken = default)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var ticketRepository = scope.ServiceProvider.GetRequiredService<ITicketRepository>();

        _logger.LogInformation("Updating database, setting GetOrganized case '{caseId}' for case with DeskproId {deskproId}", caseId, deskproId);

        var ticket = await ticketRepository.GetByDeskproTicketId(deskproId);

        if (ticket is null)
        {
            _logger.LogError("Error getting database ticket for DeskproId {id}", deskproId);
            return;
        }

        ticket.CaseNumber = caseId;
        ticket.CaseUrl = caseUrl;
        
        var rows = await ticketRepository.Update(ticket);

        if (rows != 1)
        {
            _logger.LogError("Error updating database ticket ID {id} (DeskproId: {deskproId}) setting GetOrganized CaseId '{caseId}'. {rows} ows affected", ticket.Id, deskproId, caseId, rows);
            return;
        }
    }
}