namespace AktBob.Deskpro;
internal class DeskproHandlers : IDeskproHandlers
{
    public DeskproHandlers(
        IGetDeskproMessageHandler getDeskproMessageHandler,
        IGetDeskproMessagesHandler getDeskproMessagesHandler,
        IGetDeskproPersonHandler getDeskproPersonHandler,
        IGetDeskproTicketHandler getDeskproTicketByIdHandler,
        IGetDeskproTicketsByFieldSearchHandler getDeskproTicketsByFieldSearchHandler)
    {
        GetDeskproMessage = getDeskproMessageHandler;
        GetDeskproMessages = getDeskproMessagesHandler;
        GetDeskproPerson = getDeskproPersonHandler;
        GetDeskproTicket = getDeskproTicketByIdHandler;
        GetDeskproTicketsByFieldSearch = getDeskproTicketsByFieldSearchHandler;
    }

    public IGetDeskproMessageHandler GetDeskproMessage { get; }
    public IGetDeskproMessagesHandler GetDeskproMessages { get; }
    public IGetDeskproPersonHandler GetDeskproPerson { get; }
    public IGetDeskproTicketHandler GetDeskproTicket { get; }
    public IGetDeskproTicketsByFieldSearchHandler GetDeskproTicketsByFieldSearch { get; }
}
