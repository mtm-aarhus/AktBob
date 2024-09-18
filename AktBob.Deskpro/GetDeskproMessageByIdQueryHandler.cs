using AAK.Deskpro;
using AktBob.Deskpro.Contracts;
using AktBob.Deskpro.Contracts.DTOs;
using Ardalis.Result;
using MediatR;

namespace AktBob.Deskpro;
internal class GetDeskproMessageByIdQueryHandler : IRequestHandler<GetDeskproMessageByIdQuery, Result<MessageDto>>
{
    private readonly IDeskproClient _deskproClient;

    public GetDeskproMessageByIdQueryHandler(IDeskproClient deskproClient)
    {
        _deskproClient = deskproClient;
    }

    public async Task<Result<MessageDto>> Handle(GetDeskproMessageByIdQuery request, CancellationToken cancellationToken)
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
}
