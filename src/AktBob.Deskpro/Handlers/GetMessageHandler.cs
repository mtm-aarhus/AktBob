﻿using AAK.Deskpro;

namespace AktBob.Deskpro.Handlers;
internal class GetMessageHandler(IDeskproClient deskproClient) : IGetMessageHandler
{
    private readonly IDeskproClient _deskproClient = deskproClient;

    public async Task<Result<MessageDto>> Handle(int ticketId, int messageId, CancellationToken cancellationToken)
    {
        try
        {
            var message = await _deskproClient.GetMessage(ticketId, messageId, cancellationToken);

            if (message == null)
            {
                return Result.Error($"Error getting ticket {ticketId} message {messageId} from Deskpro");
            }

            var dto = new MessageDto
            {
                Id = message.Id,
                TicketId = message.TicketId,
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

            return Result.Success(dto);
        }
        catch (HttpRequestException ex)
        {
            return Result.Error($"Error getting ticket {ticketId} message {messageId}: {ex}");
        }
        catch (Exception)
        {
            throw;
        }
    }
}