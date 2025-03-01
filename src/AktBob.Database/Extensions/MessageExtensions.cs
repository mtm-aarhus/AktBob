using AktBob.Database.Dtos;
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
            GODocumentId = message.GODocumentId,
            MessageNumber = message.MessageNumber
        };
    }
}
