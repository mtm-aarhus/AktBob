using AAK.Deskpro;
using AktBob.Deskpro.Contracts;
using AktBob.Deskpro.Contracts.DTOs;

namespace AktBob.Deskpro.UseCases;
internal class GetDeskproMessagesQueryHandler(IDeskproClient deskpro, IMediator mediator) : IRequestHandler<GetDeskproMessagesQuery, Result<IEnumerable<MessageDto>>>
{
    private readonly IDeskproClient _deskpro = deskpro;
    private readonly IMediator _mediator = mediator;

    public async Task<Result<IEnumerable<MessageDto>>> Handle(GetDeskproMessagesQuery query, CancellationToken cancellationToken)
    {
        var count = 10;
        var page = 1;
        var totalPages = 1;
        var messages = new List<MessageDto>();

        do
        {
            var deskproMessages = await _deskpro.GetTicketMessages(query.TicketId, page, count, cancellationToken);

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
            var getPersonQuery = new GetDeskproPersonQuery(message.Person.Id);
            var getPersonResult = await _mediator.Send(getPersonQuery, cancellationToken);

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
