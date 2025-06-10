using AAK.Deskpro;
using AktBob.Shared.Contracts.Modules.Deskpro.DTOs;

namespace Aktbob.Modules.Deskpro.Features.GetMessage;
internal class GetMessageHandler(IDeskproClient deskproClient) : IGetMessageHandler
{
    private readonly IDeskproClient _deskproClient = deskproClient;

    public async Task<ErrorOr<MessageDto>> Handle(int ticketId, int messageId, CancellationToken cancellationToken)
    {
        var message = await _deskproClient.GetMessage(ticketId, messageId, cancellationToken);

        if (message == null)
        {
            return Error.Failure("GetMessageHandler.Failure", $"Error getting message {messageId} from Deskpro");
        }

        return new MessageDto
        {
            Id = messageId,
            CreatedAt = message.CreatedAt,
            IsAgentNote = message.IsAgentNote,
            Content = message.Content,
            AttachmentIds = message.AttachmentIds,
            Recipients = message.Recipients,
            CreationSystem = message.CreationSystem,
            Person = new PersonDto
            {
                Id = message.Person.Id,
                IsAgent = message.Person.IsAgent,
                DisplayName = message.Person.DisplayName,
                Email = message.Person.Email,
                FirstName = message.Person.FirstName,
                LastName = message.Person.LastName,
                FullName = message.Person.FullName,
                PhoneNumbers = message.Person.PhoneNumbers
            }
        };
    }
}