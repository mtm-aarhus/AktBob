using AktBob.Deskpro.Contracts;
using AktBob.Deskpro.Handlers.DownloadMessageAttachment;
using AktBob.Deskpro.Handlers.GetCustomFieldSpecifications;
using AktBob.Deskpro.Handlers.GetMessage;
using AktBob.Deskpro.Handlers.GetMessageAttachments;
using AktBob.Deskpro.Handlers.GetMessages;
using AktBob.Deskpro.Handlers.GetPerson;
using AktBob.Deskpro.Handlers.GetTeam;
using AktBob.Deskpro.Handlers.GetTicket;
using AktBob.Deskpro.Handlers.GetTicketsByFieldSearch;
using AktBob.Deskpro.Jobs;
using AktBob.Shared;
using System.Text;
using AktBob.Deskpro.Contracts.DTOs;

namespace AktBob.Deskpro;
internal class DeskproModule(
    IJobDispatcher jobDispatcher,
    IGetCustomFieldSpecificationsHandler getCustomFieldSpecificationsHandler,
    IDownloadMessageAttachmentHandler getMessageAttachmentHandler,
    IGetMessageAttachmentsHandler getMessageAttachmentsHandler,
    IGetMessageHandler getMessageHandler,
    IGetMessagesHandler getMessagesHandler,
    IGetPersonByIdHandler getPersonByIdHandler,
    IGetPersonByEmailHandler getPersonByEmailHandler,
    IGetTicketHandler getTicketHandler,
    IGetTicketsByFieldSearchHandler getTicketsByFieldSearchHandler,
    IGetTeamHandler getTeamHandler) : IDeskproModule
{
    private readonly IGetTeamHandler _getTeamHandler = getTeamHandler;

    public void InvokeWebhook(string webhookId, string payload)
    {
        var bytes = Encoding.UTF8.GetBytes(payload);
        var base64Payload = Convert.ToBase64String(bytes);
        jobDispatcher.Dispatch(new InvokeWebhookJob(webhookId, base64Payload));
    }
    
    public async Task<ErrorOr<IReadOnlyCollection<CustomFieldSpecificationDto>>> GetCustomFieldSpecifications(CancellationToken cancellationToken) => await getCustomFieldSpecificationsHandler.Handle(cancellationToken);

    public async Task<ErrorOr<IReadOnlyCollection<AttachmentDto>>> GetMessageAttachments(int ticketId, int messageId, CancellationToken cancellationToken) => await getMessageAttachmentsHandler.Handle(ticketId, messageId, cancellationToken);

    public async Task<ErrorOr<MessageDto>> GetMessage(int ticketId, int messageId, CancellationToken cancellationToken) => await getMessageHandler.Handle(ticketId, messageId, cancellationToken);

    public async Task<ErrorOr<IReadOnlyCollection<MessageDto>>> GetMessages(int ticketId, CancellationToken cancellationToken) => await getMessagesHandler.Handle(ticketId, cancellationToken);

    public async Task<ErrorOr<Stream>> DownloadMessageAttachment(string downloadUrl, CancellationToken cancellationToken) => await getMessageAttachmentHandler.Handle(downloadUrl, cancellationToken);

    public async Task<ErrorOr<PersonDto>> GetPersonById(int personId, CancellationToken cancellationToken) => await getPersonByIdHandler.Handle(personId, cancellationToken);
    
    public async Task<ErrorOr<PersonDto>> GetPersonByEmail(string email, CancellationToken cancellationToken) => await getPersonByEmailHandler.Handle(email, cancellationToken);

    public async Task<ErrorOr<TicketDto>> GetTicket(int ticketId, CancellationToken cancellationToken) => await getTicketHandler.Handle(ticketId, cancellationToken);

    public async Task<ErrorOr<IReadOnlyCollection<TicketDto>>> GetTicketsByFieldSearch(int[] fields, string searchValue, CancellationToken cancellationToken) => await getTicketsByFieldSearchHandler.Handle(fields, searchValue, cancellationToken);

    public async Task<ErrorOr<TeamDto>> GetTeam(int teamId, CancellationToken cancellationToken) => await _getTeamHandler.Handle(teamId, cancellationToken);
}
