using AktBob.Database.Contracts;
using AktBob.Database.Entities;
using AktBob.Shared.Jobs;

namespace AktBob.Workflows.Processes;
internal class RegisterDeskproTicket : IJobHandler<RegisterDeskproTicketJob>
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<RegisterDeskproTicket> _logger;

    public RegisterDeskproTicket(IServiceScopeFactory scopeFactory, ILogger<RegisterDeskproTicket> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task Handle(RegisterDeskproTicketJob job, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Registering Deskpro ticket {id}", job.DeskproTicketId);

        using var scope = _scopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ITicketRepository>();

        var ticket = new Ticket
        {
            DeskproId = job.DeskproTicketId,
        };

        var success = await repository.Add(ticket);
        if (success)
        {
            _logger.LogInformation("Deskpro ticket {id} registered", job.DeskproTicketId);
            return;
        }

        _logger.LogError("Error registering Deskpro ticket {id}", job.DeskproTicketId);
    }
}
