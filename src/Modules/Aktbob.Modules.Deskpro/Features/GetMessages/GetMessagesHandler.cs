using AAK.Deskpro;
using Aktbob.Modules.Deskpro.Features.GetPersonById;
using AktBob.Shared.Types.Deskpro;

namespace Aktbob.Modules.Deskpro.Features.GetMessages;
internal class GetMessagesHandler(IDeskproClient deskproClient, IGetPersonByIdHandler getPersonByIdHandler) : IGetMessagesHandler
{
    private readonly IDeskproClient _deskproClient = deskproClient;
    private readonly IGetPersonByIdHandler _getPersonByIdHandler = getPersonByIdHandler;

    public async Task<ErrorOr<IReadOnlyCollection<MessageDto>>> Handle(int ticketId, CancellationToken cancellationToken)
    {
        const int count = 10;
        var page = 1;
        var totalPages = 1;
        var messages = new List<MessageDto>();

        do
        {
            var deskproMessages = await _deskproClient.GetTicketMessages(ticketId, page, count, cancellationToken);

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
            page++;

        } while (page <= totalPages);

        // Add people to the messages 
        foreach (var message in messages)
        {
            var getPersonResult = await _getPersonByIdHandler.Handle(message.Person.Id, cancellationToken);
            getPersonResult.Switch(
                value => message.Person = new PersonDto
                {
                    IsAgent = value.IsAgent,
                    DisplayName = value.DisplayName,
                    Email = value.Email,
                    FirstName = value.FirstName,
                    FullName = value.FullName,
                    Id = value.Id,
                    LastName = value.LastName,
                    PhoneNumbers = value.PhoneNumbers
                },
                _ => {}
            );
        }

        return messages;
    }
}