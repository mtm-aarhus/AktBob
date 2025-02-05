using MediatR;
using System.Collections.Concurrent;

namespace AktBob.Database.UseCases.Messages.PostMessage;
internal class PostMessageCommandHandler : IRequestHandler<PostMessageCommand>
{
    private readonly ConcurrentDictionary<Guid, DeskproTicketWithNewMessage> _dictionary;

    public PostMessageCommandHandler(ConcurrentDictionary<Guid, DeskproTicketWithNewMessage> dictionary)
    {

        _dictionary = dictionary;
    }

    public Task Handle(PostMessageCommand request, CancellationToken cancellationToken)
    {
        // Deskpro cannot post the message ID directly within its webhook
        // and the newest message is not immediately available from the Deskpro API.

        // Cache the ticket ID so the background job can handle it later
        _dictionary.TryAdd(Guid.NewGuid(), new DeskproTicketWithNewMessage(request.DeskproTicketId, DateTime.UtcNow));

        return Task.CompletedTask;
    }
}