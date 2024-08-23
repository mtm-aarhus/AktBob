using AktBob.Queue.Contracts;
using Ardalis.GuardClauses;
using Ardalis.Result;
using MediatR;

namespace AktBob.Queue.UseCases.GetQueueMessages;
internal class GetQueueMessagesQueryHandler : IRequestHandler<GetQueueMessagesQuery, Result<IEnumerable<QueueMessageDto>>>
{
    private readonly IQueue _queue;

    public GetQueueMessagesQueryHandler(IQueue queue)
    {
        _queue = queue;
    }

    public async Task<Result<IEnumerable<QueueMessageDto>>> Handle(GetQueueMessagesQuery request, CancellationToken cancellationToken)
    {

        var messages = await _queue.GetMessages(Guard.Against.NullOrEmpty(request.ConnectionString),
                                                Guard.Against.NullOrEmpty(request.QueueName),
                                                request.VisibilityvisibilyTimeoutSeconds,
                                                request.MaxMessages,
                                                cancellationToken);

        return Result.Success(messages ?? Enumerable.Empty<QueueMessageDto>());
    }
}
