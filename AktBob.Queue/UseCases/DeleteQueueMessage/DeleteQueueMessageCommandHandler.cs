using AktBob.Queue.Contracts;
using Ardalis.GuardClauses;
using MediatR;

namespace AktBob.Queue.UseCases.DeleteQueueMessage;
internal class DeleteQueueMessageCommandHandler : IRequestHandler<DeleteQueueMessageCommand>
{
    private readonly IQueue _queue;

    public DeleteQueueMessageCommandHandler(IQueue queue)
    {
        _queue = queue;
    }

    public async Task Handle(DeleteQueueMessageCommand request, CancellationToken cancellationToken)
    {
        await _queue.DeleteMessage(Guard.Against.NullOrEmpty(request.ConnectionString),
                                   Guard.Against.NullOrEmpty(request.QueueName),
                                   Guard.Against.NullOrEmpty(request.MessageId),
                                   Guard.Against.NullOrEmpty(request.PopReciept),
                                   cancellationToken);
    }
}
