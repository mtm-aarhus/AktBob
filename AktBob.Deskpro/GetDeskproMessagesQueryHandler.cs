using AAK.Deskpro;
using AktBob.Deskpro.Contracts;
using AktBob.Deskpro.Contracts.DTOs;
using MassTransit;
using MassTransit.Mediator;

namespace AktBob.Deskpro;
public class GetDeskproMessagesQueryHandler(IDeskproClient deskpro, IMediator mediator) : MediatorRequestHandler<GetDeskproMessagesQuery, IEnumerable<MessageDto>>
{
    private readonly IDeskproClient _deskpro = deskpro;
    private readonly IMediator _mediator = mediator;

    protected override async Task<IEnumerable<MessageDto>> Handle(GetDeskproMessagesQuery query, CancellationToken cancellationToken)
    {
        var count = 10;
        var page = 1;
        var totalPages = 1;
        var dtos = new List<MessageDto>();

        do
        {
            var messages = await _deskpro.GetTicketMessages(query.TicketId, page, count, cancellationToken);

            if (messages != null)
            {
                dtos.AddRange(messages.Data.Select(x => new MessageDto
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
                
                totalPages = messages.Pagination.TotalPages;
            }

            page++;

        } while (page <= totalPages);


        // Add people to the messages 
        foreach (var dto in dtos)
        {
            var getPersonQuery = new GetDeskproPersonQuery(dto.Person.Id);
            var getPersonResult = await _mediator.SendRequest(getPersonQuery, cancellationToken);

            var person = getPersonResult.Value;
            if (person != null)
            {
                dto.Person = new PersonDto
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

        return dtos;
    }
}
