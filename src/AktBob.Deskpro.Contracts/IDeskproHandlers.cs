namespace AktBob.Deskpro.Contracts;
public interface IDeskproHandlers
{
    IGetDeskproCustomFieldSpecificationsHandler GetDeskproCustomFieldSpecifications { get; }
    IGetDeskproMessageAttachmentHandler GetDeskproMessageAttachment { get; }
    IGetDeskproMessageAttachmentsHandler GetDeskproMessageAttachments { get; }
    IGetDeskproMessageHandler GetDeskproMessage { get; }
    IGetDeskproMessagesHandler GetDeskproMessages { get; }
    IGetDeskproPersonHandler GetDeskproPerson { get; }
    IGetDeskproTicketHandler GetDeskproTicket { get; }
    IGetDeskproTicketsByFieldSearchHandler GetDeskproTicketsByFieldSearch { get; }
    IInvokeDeskproWebhookHandler InvokeDeskproWebhook { get; }
}