using AktBob.Database.Contracts;
using AktBob.Shared.Jobs;

namespace AktBob.Workflows.Processes;
internal class UpdateDatabaseTicket : IJobHandler<UpdateDatabaseTicketJob>
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<UpdateDatabaseTicket> _logger;

    public UpdateDatabaseTicket(IServiceScopeFactory scopeFactory, ILogger<UpdateDatabaseTicket> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task Handle(UpdateDatabaseTicketJob job, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating database ticket {id}", job.Id);

        using var scope = _scopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ITicketRepository>();

        // Get existing entity from repository
        var ticket = await repository.Get(job.Id);
        if (ticket == null) throw new BusinessException($"Error updating database ticket {job.Id}: not found in database.");

        // Update entity properties
        if (!string.IsNullOrEmpty(job.CaseNumber))
        {
            ticket.CaseNumber = job.CaseNumber;
        }

        if (!string.IsNullOrEmpty(job.CaseUrl))
        {
            ticket.CaseUrl = job.CaseUrl;
        }

        if (!string.IsNullOrEmpty(job.SharepointFolderName))
        {
            ticket.SharepointFolderName = job.SharepointFolderName;
        }

        // Update
        var updated = await repository.Update(ticket);

        // Response
        if (updated)
        {
            _logger.LogInformation("Database ticket {id} updated: {ticket}", job.Id, ticket);
            return;
        }

        _logger.LogError("Error updating database ticket {id}", job.Id);
    }
}
