using AktBob.Database.Dtos;
using AktBob.Database.Entities;

namespace AktBob.Database.Extensions;
internal static class TicketExtensions
{
    public static TicketDto ToDto(this Ticket ticket)
    {
        var dto = new TicketDto
        {
            Id = ticket.Id,
            DeskproId = ticket.DeskproId,
            CaseNumber = ticket.CaseNumber,
            CaseUrl = ticket.CaseUrl,
            SharepointFolderName = ticket.SharepointFolderName,
            Cases = ticket.Cases.Select(c => c.ToDto())
        };

        return dto;
    }

    public static IEnumerable<TicketDto> ToDto(this IEnumerable<Ticket> tickets)
    {
        var dtos = tickets.Select(ticket => ticket.ToDto());
        return dtos;
    }
}
