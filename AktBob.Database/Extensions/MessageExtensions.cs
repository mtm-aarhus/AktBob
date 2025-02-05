using AktBob.Database.Contracts.Dtos;
using AktBob.Database.Entities;

namespace AktBob.Database.Extensions;
internal static class MessageExtensions
{
    public static MessageDto ToDto(this Message message)
    {
        return new MessageDto
        {
            Id = message.Id,
            TicketId = message.TicketId,
            DeskproMessageId = message.DeskproMessageId,
            DeskproTicketId = message.DeskproTicketId,
            GOCaseNumber = message.GOCaseNumber,
            GODocumentId = message.GODocumentId,
            Hash = message.Hash,
            QueuedForJournalizationAt = message.QueuedForJournalizationAt,
            MessageNumber = message.MessageNumber
        };
    }
}
