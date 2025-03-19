using AktBob.Database.Contracts;
using AktBob.Database.Entities;
using AktBob.Deskpro.Contracts;
using AktBob.Shared.Jobs;

namespace AktBob.Workflows.Processes.AddMessageToGetOrganized;
internal class RegisterMessages(ILogger<RegisterMessages> logger, IServiceScopeFactory serviceScopeFactory) : IJobHandler<RegisterMessagesJob>
{
    private readonly ILogger<RegisterMessages> _logger = logger;
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;

    public async Task Handle(RegisterMessagesJob job, CancellationToken cancellationToken = default)
    {
        // Validate job parameters
        Guard.Against.NegativeOrZero(job.DeskproTicketId);

        var scope = _serviceScopeFactory.CreateScope();
        var jobDispatcher = scope.ServiceProvider.GetRequiredService<IJobDispatcher>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var deskpro = scope.ServiceProvider.GetRequiredService<IDeskproModule>();

        // Get message from Deskpro
        var getDeskproMessagesResult = await deskpro.GetMessages(job.DeskproTicketId, cancellationToken);
        if (!getDeskproMessagesResult.IsSuccess) throw new BusinessException("Unable to get messages from Deskpro.");

        // Persist the Deskpro ticket ID and message ID in the database
        var tasks = getDeskproMessagesResult.Value.Select(async deskproMessage =>
        {
            var databaseTicket = await unitOfWork.Tickets.GetByDeskproTicketId(job.DeskproTicketId);
            if (databaseTicket is null) throw new BusinessException("Unable to get ticket from database.");

            var existingMessage = await unitOfWork.Messages.GetByDeskproMessageId(deskproMessage.Id);
            if (existingMessage is null)
            {
                var message = new Message
                {
                    TicketId = databaseTicket.Id,
                    DeskproMessageId = deskproMessage.Id,
                };

                if (!await unitOfWork.Messages.Add(message)) throw new BusinessException($"Unable to add new message to database (TicketId = {databaseTicket.Id}, DeskproMessageId = {deskproMessage.Id})");
            }

            if ((existingMessage is null || existingMessage.GODocumentId is null) && !string.IsNullOrEmpty(databaseTicket.CaseNumber))
            {
                jobDispatcher.Dispatch(new AddMessageToGetOrganizedJob(deskproMessage.Id, databaseTicket.CaseNumber));
            }

            return Task.CompletedTask;
        });

        await Task.WhenAll(tasks);
    }
}
