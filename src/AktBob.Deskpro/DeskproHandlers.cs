namespace AktBob.Deskpro;
internal class DeskproHandlers : IDeskproHandlers
{
    public DeskproHandlers(
        IGetDeskproCustomFieldSpecificationsHandler deskproCustomFieldSpecificationsHandler,
        IGetDeskproMessageAttachmentHandler getDeskproMessageAttachmentHandler,
        IGetDeskproMessageAttachmentsHandler getDeskproMessageAttachmentsHandler,
        IGetDeskproMessageHandler getDeskproMessageHandler,
        IGetDeskproMessagesHandler getDeskproMessagesHandler,
        IGetDeskproPersonHandler getDeskproPersonHandler,
        IGetDeskproTicketHandler getDeskproTicketByIdHandler,
        IGetDeskproTicketsByFieldSearchHandler getDeskproTicketsByFieldSearchHandler,
        IInvokeDeskproWebhookHandler invokeDeskproWebhookHandler)
    {
        GetDeskproCustomFieldSpecifications = deskproCustomFieldSpecificationsHandler;
        GetDeskproMessageAttachment = getDeskproMessageAttachmentHandler;
        GetDeskproMessageAttachments = getDeskproMessageAttachmentsHandler;
        GetDeskproMessage = getDeskproMessageHandler;
        GetDeskproMessages = getDeskproMessagesHandler;
        GetDeskproPerson = getDeskproPersonHandler;
        GetDeskproTicket = getDeskproTicketByIdHandler;
        GetDeskproTicketsByFieldSearch = getDeskproTicketsByFieldSearchHandler;
        InvokeDeskproWebhook = invokeDeskproWebhookHandler;
    }

    public IGetDeskproCustomFieldSpecificationsHandler GetDeskproCustomFieldSpecifications { get; }
    public IGetDeskproMessageAttachmentHandler GetDeskproMessageAttachment { get; }
    public IGetDeskproMessageAttachmentsHandler GetDeskproMessageAttachments { get; }
    public IGetDeskproMessageHandler GetDeskproMessage { get; }
    public IGetDeskproMessagesHandler GetDeskproMessages { get; }
    public IGetDeskproPersonHandler GetDeskproPerson { get; }
    public IGetDeskproTicketHandler GetDeskproTicket { get; }
    public IGetDeskproTicketsByFieldSearchHandler GetDeskproTicketsByFieldSearch { get; }
    public IInvokeDeskproWebhookHandler InvokeDeskproWebhook { get; }
}
