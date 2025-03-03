namespace AktBob.Deskpro.Contracts;
public interface IDeskproHandlers
{
    IGetDeskproMessageHandler GetDeskproMessage { get; }
    IGetDeskproMessagesHandler GetDeskproMessages { get; }
    IGetDeskproPersonHandler GetDeskproPerson { get; }
    IGetDeskproTicketHandler GetDeskproTicket { get; }
    IGetDeskproTicketsByFieldSearchHandler GetDeskproTicketsByFieldSearch { get; }
}