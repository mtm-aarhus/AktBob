namespace AktBob.Database;

internal static class Constants
{
    // Stored procedure names
    public const string SP_CASE_CREATE = "spCase_Create";
    public const string SP_CASE_GET_BY_TICKET_ID = "spCase_GetByTicketId";
    public const string SP_CASE_GET_BY_ID = "spCase_GetById";
    public const string SP_CASE_UPDATE_BY_ID = "spCase_UpdateById";

    public const string SP_TICKET_CREATE = "spTicket_Create";
    public const string SP_TICKET_GET_BY_DESKPRO_ID = "spTicket_GetByDeskproId";
    public const string SP_TICKET_GET_BY_ID = "spTicket_GetById";
    public const string SP_TICKET_UPDATE_BY_ID = "spTicket_UpdateById";

    public const string SP_MESSAGE_CREATE = "spMessage_Create";
    public const string SP_MESSAGE_UPDATE = "spMessage_Update";
    public const string SP_MESSAGE_GET_ALL_NOT_JOURNALIZED = "spMessage_GetAllNotJournalized";
    public const string SP_MESSAGE_GET_ALL = "spMessage_GetAll";
    public const string SP_MESSAGE_GET_BY_ID = "spMessage_GetById";
    public const string SP_MESSAGE_DELETE = "spMessage_Delete";
    public const string SP_MESSAGE_CLEAR_QUEUED_FOR_JOURNALIZATION = "spMessage_ClearQueuedForJournalization";
    public const string SP_MESSAGE_GET_BY_DESKPRO_MESSAGE_ID = "spMessage_GetByDeskproMessageId";

    // Views
    public const string V_TICKETS = "v_Tickets";
    public const string V_CASES = "v_Cases";
    public const string V_MESSAGES = "v_Messages";

    // "Tickets" table column names
    public const string T_TICKETS = "Tickets";
    public const string T_TICKETS_ID = "Id";
    public const string T_TICKETS_CASENUMBER = "CaseNumber";
    public const string T_TICKETS_DESKPRO_ID = "DeskproId";
    public const string T_TICKETS_SHAREPOINT_FOLDERNAME = "SharepointFolderName";
    public const string T_TICKETS_JOURNALIZED_AT = "JournalizedAt";
    public const string T_TICKETS_CLOSED_AT = "TicketClosedAt";
    public const string T_TICKETS_CASEURL = "CaseUrl";



    // "Cases" table column names
    public const string T_CASES = "Cases";
    public const string T_CASES_ID = "Id";
    public const string T_CASES_TICKET_ID = "TicketId";
    public const string T_CASES_CASENUMBER = "CaseNumber";
    public const string T_CASES_PODIO_ITEM_ID = "PodioItemId";
    public const string T_CASES_FILARKIV_CASE_ID = "FilArkivCaseId";
    public const string T_CASES_SHAREPOINT_FOLDERNAME = "SharepointFolderName";


    // "Messages" table column names
    public const string T_MESSAGES = "Messages";
    public const string T_MESSAGES_ID = "Id";
    public const string T_MESSAGES_TICKET_ID = "TicketId";
    public const string T_MESSAGES_GO_DOCUMENT_ID = "GODocumentId";
    public const string T_MESSAGES_DESKPRO_ID = "DeskproId";
    public const string T_MESSAGES_QUEUED_FOR_JOURNALIZATION_AT = "QueuedForJournalizationAt";
    public const string T_MESSAGES_HASH = "Hash";
}