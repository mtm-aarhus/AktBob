using AktBob.Database.Contracts;
using AktBob.Database.Entities;
using AktBob.Deskpro.Contracts;
using AktBob.Shared.Extensions;
using AktBob.Shared.Jobs;

namespace AktBob.Workflows.Processes.AddMessageToGetOrganized;
internal class RegisterMessages(IServiceScopeFactory serviceScopeFactory) : IJobHandler<RegisterMessagesJob>
{
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;

    public async Task Handle(RegisterMessagesJob job, CancellationToken cancellationToken = default)
    {
        var scope = _serviceScopeFactory.CreateScope();
        var jobDispatcher = scope.ServiceProvider.GetRequiredServiceOrThrow<IJobDispatcher>();
        var unitOfWork = scope.ServiceProvider.GetRequiredServiceOrThrow<IUnitOfWork>();
        var deskpro = scope.ServiceProvider.GetRequiredServiceOrThrow<IDeskproModule>();

        // Get message from Deskpro
        var getDeskproMessagesResult = await deskpro.GetMessages(job.TicketId, cancellationToken);
        if (getDeskproMessagesResult.IsError) throw new BusinessException("Unable to get messages from Deskpro.");

        // Persist the Deskpro ticket ID and message ID in the database
        var tasks = getDeskproMessagesResult.Value.Select(async deskproMessage =>
        {
            var databaseTicket = await unitOfWork.Tickets.GetByDeskproTicketId(job.TicketId);
            if (databaseTicket is null) throw new BusinessException("Unable to get ticket from database.");

            var existingMessage = await unitOfWork.Messages.GetByDeskproMessageId(deskproMessage.Id.Id);
            if (existingMessage is null)
            {
                var message = new Message
                {
                    TicketId = databaseTicket.Id,
                    DeskproMessageId = deskproMessage.Id.Id,
                };

                if (!await unitOfWork.Messages.Add(message)) throw new BusinessException($"Unable to add new message to database (TicketId = {databaseTicket.Id}, DeskproMessageId = {deskproMessage.Id})");
            }

            if ((existingMessage is null || existingMessage.GODocumentId is null) && !string.IsNullOrEmpty(databaseTicket.CaseNumber))
            {
                jobDispatcher.Dispatch(new  AddMessageToGetOrganizedJob(deskproMessage.Id, databaseTicket.CaseNumber));
            }

            return Task.CompletedTask;
        });

        await Task.WhenAll(tasks);
    }
}
