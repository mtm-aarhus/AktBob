using AktBob.Queue.Contracts;
using Ardalis.GuardClauses;
using Ardalis.Result;
using MassTransit.Mediator;

namespace AktBob.Queue.UseCases;
internal class GetQueueMessagesQueryHandler(IQueue queue) : MediatorRequestHandler<GetQueueMessagesQuery, Result<IEnumerable<QueueMessageDto>>>
{
    private readonly IQueue _queue = queue;

    protected override async Task<Result<IEnumerable<QueueMessageDto>>> Handle(GetQueueMessagesQuery request, CancellationToken cancellationToken)
    {

        var messages = await _queue.GetMessages(Guard.Against.NullOrEmpty(request.QueueName),
                                                request.VisibilityvisibilyTimeoutSeconds,
                                                request.MaxMessages,
                                                cancellationToken);

        return Result.Success(messages ?? Enumerable.Empty<QueueMessageDto>());
    }
}
