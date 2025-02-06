using MassTransit.Mediator;
using System.Collections.Concurrent;

namespace AktBob.Database.UseCases.Messages.PostMessage;

public record PostMessageCommand(int DeskproTicketId);
public class PostMessageCommandHandler(ConcurrentDictionary<Guid, DeskproTicketWithNewMessage> dictionary) : MediatorRequestHandler<PostMessageCommand>
{
    private readonly ConcurrentDictionary<Guid, DeskproTicketWithNewMessage> _dictionary = dictionary;

    protected override Task Handle(PostMessageCommand request, CancellationToken cancellationToken)
    {
        // Deskpro cannot post the message ID directly within its webhook
        // and the newest message is not immediately available from the Deskpro API.

        // Cache the ticket ID so the background job can handle it later
        _dictionary.TryAdd(Guid.NewGuid(), new DeskproTicketWithNewMessage(request.DeskproTicketId, DateTime.UtcNow));

        return Task.CompletedTask;
    }
}