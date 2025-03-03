using AktBob.Database.Contracts;
using AktBob.Database.Jobs;
using AktBob.Shared;
using Microsoft.Extensions.DependencyInjection;

namespace AktBob.Database.JobHandlers;
internal class DeleteMessage(IServiceScopeFactory serviceScopeFactory) : IJobHandler<DeleteMessageJob>
{
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;

    public async Task Handle(DeleteMessageJob job, CancellationToken cancellationToken = default)
    {
        var scope = _serviceScopeFactory.CreateScope();
        var messageRepository = scope.ServiceProvider.GetRequiredService<IMessageRepository>();
        await messageRepository.Delete(job.MessageId);
    }
}