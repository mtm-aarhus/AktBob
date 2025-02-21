using AktBob.Database.Contracts.Messages;

namespace AktBob.JobHandlers.Handlers.AddMessageToGetOrganized;
internal class DeleteMessage(IServiceScopeFactory serviceScopeFactory)
{
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;

    public async Task Run(int id, CancellationToken cancellationToken = default)
    {
        var scope = _serviceScopeFactory.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var deleteMessageCommand = new DeleteMessageCommand(id);
        await mediator.Send(deleteMessageCommand, cancellationToken);
    }
}
