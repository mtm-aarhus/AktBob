using AktBob.Database.Contracts.Messages;

namespace AktBob.JobHandlers.Handlers.AddMessageToGetOrganized;
internal class DeleteMessage(IServiceScopeFactory serviceScopeFactory)
{
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;

    public async Task Run(int id, CancellationToken cancellationToken = default)
    {
        var scope = _serviceScopeFactory.CreateScope();
        var commandDispatcher = scope.ServiceProvider.GetRequiredService<ICommandDispatcher>();

        var deleteMessageCommand = new DeleteMessageCommand(id);
        await commandDispatcher.Dispatch(deleteMessageCommand, cancellationToken);
    }
}
