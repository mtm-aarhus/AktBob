using AAK.Deskpro;

namespace AktBob.Deskpro.Handlers;
internal class GetDeskproMessagesHandler(IDeskproClient deskpro, IGetDeskproPersonHandler getDeskproPersonHandler) : IGetDeskproMessagesHandler
{
    private readonly IDeskproClient _deskpro = deskpro;
    private readonly IGetDeskproPersonHandler _getDeskproPersonHandler = getDeskproPersonHandler;

    public async Task<Result<IEnumerable<MessageDto>>> Handle(int ticketId, CancellationToken cancellationToken)
    {
        var count = 10;
        var page = 1;
        var totalPages = 1;
        var messages = new List<MessageDto>();

        do
        {
            var deskproMessages = await _deskpro.GetTicketMessages(ticketId, page, count, cancellationToken);

            if (deskproMessages != null)
            {
                messages.AddRange(deskproMessages.Data.Select(x => new MessageDto
                {
                    AttachmentIds = x.AttachmentIds,
                    CreatedAt = x.CreatedAt,
                    IsAgentNote = x.IsAgentNote,
                    Content = x.Content,
                    Id = x.Id,
                    Person = new PersonDto
                    {
                        Id = x.Person.Id,
                    },
                    TicketId = x.TicketId
                }));

                totalPages = deskproMessages.Pagination.TotalPages;
            }

            page++;

        } while (page <= totalPages);


        // Add people to the messages 
        foreach (var message in messages)
        {
            var getPersonResult = await _getDeskproPersonHandler.Handle(message.Person.Id, cancellationToken);

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