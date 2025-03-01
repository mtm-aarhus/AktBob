using AktBob.Database.Contracts;

namespace AktBob.JobHandlers.Handlers.AddMessageToGetOrganized;
internal class DeleteMessage(IServiceScopeFactory serviceScopeFactory)
{
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;

    public async Task Run(int id, CancellationToken cancellationToken = default)
    {
        var scope = _serviceScopeFactory.CreateScope();
        var messageRepository = scope.ServiceProvider.GetRequiredService<IMessageRepository>();
        await messageRepository.Delete(id);
    }
}