using System.Diagnostics.CodeAnalysis;

namespace AktBob.Shared.Types.Deskpro;
public struct MessageId : IEquatable<MessageId>
{
    public int TicketId { get; }
    public int Id { get; }

    public MessageId(int ticketId, int messageId) => (TicketId, Id) = (ticketId, messageId);

    public static MessageId Create(int ticketId, int messageId) => new MessageId(ticketId, messageId);
    public override string ToString() => $"(TicketId = {TicketId}, MessageId = {Id})";
    public override bool Equals([NotNullWhen(true)] object? obj) => obj is MessageId other && Equals(other);
    public bool Equals(MessageId other) => TicketId == other.TicketId && Id == other.Id;
    public override int GetHashCode()
    {
        return HashCode.Combine(TicketId, Id);
    }
}