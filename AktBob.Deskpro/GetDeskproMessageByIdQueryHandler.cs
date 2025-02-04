using AAK.Deskpro;
using AktBob.Deskpro.Contracts;
using AktBob.Deskpro.Contracts.DTOs;
using Ardalis.Result;
using MassTransit.Mediator;

namespace AktBob.Deskpro;
public class GetDeskproMessageByIdQueryHandler(IDeskproClient deskproClient) : MediatorRequestHandler<GetDeskproMessageByIdQuery, Result<MessageDto>>
{
    private readonly IDeskproClient _deskproClient = deskproClient;

    protected override async Task<Result<MessageDto>> Handle(GetDeskproMessageByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var message = await _deskproClient.GetMessage(request.TicketId, request.MessageId, cancellationToken);

            if (message == null)
            {
                return Result.NotFound();
            }

            var dto = new MessageDto
            {
                Id = message.Id,
                TicketId = message.TicketId,
                CreatedAt = message.CreatedAt,
                IsAgentNote = message.IsAgentNote,
                Content = message.Content,
                AttachmentIds = message.AttachmentIds,
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
            
            return Result.Success(dto);
        }
        catch (Exception)
        {
            return Result.Error();
        }
    }
}
