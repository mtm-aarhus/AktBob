using AAK.Deskpro;
using AktBob.Deskpro.Contracts;
using AktBob.Deskpro.Contracts.DTOs;

namespace AktBob.Deskpro.UseCases;
internal class GetDeskproMessageByIdQueryHandler(IDeskproClient deskproClient) : IQueryHandler<GetDeskproMessageByIdQuery, Result<MessageDto>>
{
    private readonly IDeskproClient _deskproClient = deskproClient;

    public async Task<Result<MessageDto>> Handle(GetDeskproMessageByIdQuery request, CancellationToken cancellationToken)
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
