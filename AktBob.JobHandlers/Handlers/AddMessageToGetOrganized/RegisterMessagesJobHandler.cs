using AktBob.Database.Contracts;
using AktBob.Database.Contracts.Messages;
using AktBob.Deskpro.Contracts;
using AktBob.Shared.Contracts;

namespace AktBob.JobHandlers.Handlers.AddMessageToGetOrganized;
internal class RegisterMessagesJobHandler(ILogger<RegisterMessagesJobHandler> logger, IServiceScopeFactory serviceScopeFactory, IConfiguration configuration) : IJobHandler<RegisterMessagesJob>
{
    private readonly ILogger<RegisterMessagesJobHandler> _logger = logger;
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;
    private readonly IConfiguration _configuration = configuration;

    public async Task Handle(RegisterMessagesJob job, CancellationToken cancellationToken = default)
    {
        var scope = _serviceScopeFactory.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var getDeskproMessagesQuery = new GetDeskproMessagesQuery(job.DeskproTicketId);
        var getDeskproMessagesResult = await mediator.SendRequest(getDeskproMessagesQuery, cancellationToken);

        if (!getDeskproMessagesResult.IsSuccess)
        {
            _logger.LogError("Error getting messages from Deskpro Ticket {id}", job.DeskproTicketId);
            return;
        }

        var deskproMessages = getDeskproMessagesResult.Value;

        // Persist the Deskpro ticket ID and message ID in the database
        foreach (var deskproMessage in deskproMessages)
        {
            var getDatabaseTicketsQuery = new GetTicketsQuery(job.DeskproTicketId, null, null, true);
            var getDatabaseTicketsResult = await mediator.SendRequest(getDatabaseTicketsQuery, cancellationToken);

            if (!getDatabaseTicketsResult.IsSuccess || !getDatabaseTicketsResult.Value.Any())
            {
                _logger.LogError("Error getting database ticket for DeskproTicketId {id}", job.DeskproTicketId);
                return;
            }

            if (getDatabaseTicketsResult.Value.Count() > 1)
            {
                _logger.LogError("More than 1 row in tickets table with Deskpro ticket ID {id}", job.DeskproTicketId);
                return;
            }

            var databaseTicket = getDatabaseTicketsResult.Value.First();

            var addMessageCommand = new AddMessageCommand(databaseTicket.Id, deskproMessage.Id); // The handler's stored procedure prevents from persisting duplicates, so we don't need to worry about it here
            var addMessageResult = await mediator.SendRequest(addMessageCommand, cancellationToken);

            if (!addMessageResult.IsSuccess)
            {
                _logger.LogError("Error adding message to database. Deskpro ticket Id {ticektId} Deskpro message Id {messageId}", job.DeskproTicketId, deskproMessage.Id);
                return;
            }

            var databaseMessageId = addMessageResult.Value;

            if (!string.IsNullOrEmpty(databaseTicket.CaseNumber))
            {
                BackgroundJob.Enqueue<AddMessageToGetOrganized>(x => x.Run(deskproMessage.Id, databaseTicket.CaseNumber, CancellationToken.None));
            }
        }
    }
}
