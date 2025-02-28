using AktBob.Database.Contracts;
using AktBob.Database.UseCases.Tickets.UpdateTicket;

namespace AktBob.JobHandlers.Handlers.CreateGetOrganizedCase;
internal class UpdateDatabase(ILogger<UpdateDatabase> logger, IServiceScopeFactory serviceScopeFactory)
{
    private readonly ILogger<UpdateDatabase> _logger = logger;
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;

    public async Task SetGetOrganizedCaseId(int deskproId, string caseId, string caseUrl, CancellationToken cancellationToken = default)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        _logger.LogInformation("Updating database, setting GetOrganized case '{caseId}' for case with DeskproId {deskproId}", caseId, deskproId);

        var getDatabaseTicketQuery = new GetTicketsQuery(deskproId, null, null);
        var getDatabaseTicketResult = await mediator.Send(getDatabaseTicketQuery, cancellationToken);

        if (!getDatabaseTicketResult.IsSuccess || getDatabaseTicketResult.Value is null)
        {
            _logger.LogError("Error getting database ticket for DeskproId {id}", deskproId);
            return;
        }

        var databaseTicket = getDatabaseTicketResult.Value.First();

        var updateDatabaseTicketCommand = new UpdateTicketCommand(databaseTicket.Id, caseId, caseUrl, null, null, null);
        var updateDatabaseTicketResult = await mediator.Send(updateDatabaseTicketCommand, cancellationToken);

        if (!updateDatabaseTicketResult.IsSuccess)
        {
            _logger.LogError("Error updating database ticket ID {id} (DeskproId: {deskproId}) setting GetOrganized CaseId '{caseId}'", databaseTicket.Id, deskproId, caseId);
            return;
        }
    }
}
