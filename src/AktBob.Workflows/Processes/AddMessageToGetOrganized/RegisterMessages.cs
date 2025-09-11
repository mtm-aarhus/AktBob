using AktBob.Database.Contracts;
using AktBob.Database.Entities;
using Aktbob.Modules.Deskpro.Features.GetMessages;
using AktBob.Shared.Contracts.Processors.GetOrganized;
using AktBob.Shared.Extensions;

namespace AktBob.Workflows.Processes.AddMessageToGetOrganized;
internal class RegisterMessages(IServiceScopeFactory serviceScopeFactory, ILogger<RegisterMessages> logger) : IJobHandler<RegisterMessagesJob>
{
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;
    private readonly ILogger<RegisterMessages> _logger = logger;

    public async Task Handle(RegisterMessagesJob job, CancellationToken cancellationToken = default)
    {
        var scope = _serviceScopeFactory.CreateScope();
        var jobDispatcher = scope.ServiceProvider.GetRequiredServiceOrThrow<IJobDispatcher>();
        var unitOfWork = scope.ServiceProvider.GetRequiredServiceOrThrow<IUnitOfWork>();
        var deskproGetMessagesHandler = scope.ServiceProvider.GetRequiredServiceOrThrow<IGetMessagesHandler>();
        
        _logger.LogInformation("Registering messages for Deskpro ticket {id}", job.TicketId);
        
        // Get message from Deskpro
        var getDeskproMessagesResult = await deskproGetMessagesHandler.Handle(job.TicketId, cancellationToken);
        if (getDeskproMessagesResult.IsError) throw new BusinessException("Unable to get messages from Deskpro.");

        // Persist the Deskpro ticket ID and message ID in the database
        var tasks = getDeskproMessagesResult.Value.Select(async deskproMessage =>
        {
            var databaseTicket = await unitOfWork.Tickets.GetByDeskproTicketId(job.TicketId);
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
                _logger.LogInformation("Deskpro message {messageId} added to database", message.Id);
            }

            if (existingMessage?.GODocumentId is null && !string.IsNullOrEmpty(databaseTicket.CaseNumber))
            {
                jobDispatcher.Dispatch(new AddMessageToGetOrganizedJob(deskproMessage.TicketId, deskproMessage.Id, databaseTicket.CaseNumber));
            }

            return Task.CompletedTask;
        });

        await Task.WhenAll(tasks);
    }
}
