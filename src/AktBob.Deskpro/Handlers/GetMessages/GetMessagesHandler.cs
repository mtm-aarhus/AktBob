using AAK.Deskpro;
using AktBob.Deskpro.Contracts;
using AktBob.Shared.Types.Deskpro;

namespace AktBob.Deskpro.Handlers.GetMessages;
internal class GetMessagesHandler(IDeskproClient deskproClient, IGetPersonByIdHandler getPersonByIdHandler) : IGetMessagesHandler
{
    private readonly IDeskproClient _deskproClient = deskproClient;
    private readonly IGetPersonByIdHandler _getPersonByIdHandler = getPersonByIdHandler;

    public async Task<ErrorOr<IReadOnlyCollection<MessageDto>>> Handle(int ticketId, CancellationToken cancellationToken)
    {
        var count = 10;
        var page = 1;
        var totalPages = 1;
        var messages = new List<MessageDto>();

        do
        {
            var deskproMessages = await _deskproClient.GetTicketMessages(ticketId, page, count, cancellationToken);

            if (deskproMessages != null)
            {
                messages.AddRange(deskproMessages.Data.Select(x => new MessageDto
                {
                    Id = MessageId.Create(ticketId, x.Id),
                    AttachmentIds = x.AttachmentIds,
                    CreatedAt = x.CreatedAt,
                    IsAgentNote = x.IsAgentNote,
                    Content = x.Content,
                    Recipients = x.Recipients,
                    CreationSystem = x.CreationSystem,
                    Person = new PersonDto
                    {
                        Id = x.Person.Id,
                    }
                }));

                totalPages = deskproMessages.Pagination.TotalPages;
            }

            page++;

        } while (page <= totalPages);

        if (messages is null)
        {
            return Error.Failure("GetMessagesHandler.Failure", $"Error getting ticket {ticketId} messages");
        }

        // Add people to the messages 
        foreach (var message in messages)
        {
            var getPersonResult = await _getPersonByIdHandler.Handle(message.Person.Id, cancellationToken);

            var person = getPersonResult.Value;
            if (person != null)
            {
                message.Person = new PersonDto
                {
                    IsAgent = person.IsAgent,
                    DisplayName = person.DisplayName,
                    Email = person.Email,
                    FirstName = person.FirstName,
                    FullName = person.FullName,
                    Id = person.Id,
                    LastName = person.LastName,
                    PhoneNumbers = person.PhoneNumbers
                };
            }
        }

        return messages;
    }
}