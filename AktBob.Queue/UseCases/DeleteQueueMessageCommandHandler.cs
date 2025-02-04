using AktBob.Queue.Contracts;
using Ardalis.GuardClauses;
using MassTransit.Mediator;

namespace AktBob.Queue.UseCases;
public class DeleteQueueMessageCommandHandler(IQueue queue) : MediatorRequestHandler<DeleteQueueMessageCommand>
{
    private readonly IQueue _queue = queue;

    protected override async Task Handle(DeleteQueueMessageCommand request, CancellationToken cancellationToken)
    {
        await _queue.DeleteMessage(Guard.Against.NullOrEmpty(request.QueueName),
                                   Guard.Against.NullOrEmpty(request.MessageId),
                                   Guard.Against.NullOrEmpty(request.PopReciept),
                                   cancellationToken);
    }
}
