using AktBob.Database.Contracts;
using AktBob.Database.Entities;
using AktBob.Deskpro.Contracts;
using AktBob.Shared.Jobs;

namespace AktBob.JobHandlers.Processes.AddMessageToGetOrganized;
internal class RegisterMessages(ILogger<RegisterMessages> logger, IServiceScopeFactory serviceScopeFactory) : IJobHandler<RegisterMessagesJob>
{
    private readonly ILogger<RegisterMessages> _logger = logger;
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;

    public async Task Handle(RegisterMessagesJob job, CancellationToken cancellationToken = default)
    {
        var scope = _serviceScopeFactory.CreateScope();
        var jobDispatcher = scope.ServiceProvider.GetRequiredService<IJobDispatcher>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var deskpro = scope.ServiceProvider.GetRequiredService<IDeskproModule>();


        // Get message from Deskpro
        var getDeskproMessagesResult = await deskpro.GetMessages(job.DeskproTicketId, cancellationToken);

        if (!getDeskproMessagesResult.IsSuccess)
        {
            _logger.LogError("Error getting messages from Deskpro Ticket {id}", job.DeskproTicketId);
            return;
        }


        // Persist the Deskpro ticket ID and message ID in the database
        foreach (var deskproMessage in getDeskproMessagesResult.Value)
        {
            var databaseTicket = await unitOfWork.Tickets.GetByDeskproTicketId(job.DeskproTicketId);

            if (databaseTicket is null)
            {
                _logger.LogError("Error getting database ticket for DeskproTicketId {id}", job.DeskproTicketId);
                return;
            }

            var message = new Message
            {
                TicketId = databaseTicket.Id,
                DeskproMessageId = deskproMessage.Id,
            };

            var databaseMessageId = await unitOfWork.Messages.Add(message);

            if (!string.IsNullOrEmpty(databaseTicket.CaseNumber))
            {
                jobDispatcher.Dispatch(new AddMessageToGetOrganizedJob(deskproMessage.Id, databaseTicket.CaseNumber));
            }
        }
    }
}
