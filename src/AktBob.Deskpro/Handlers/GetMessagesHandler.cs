using AAK.Deskpro;

namespace AktBob.Deskpro.Handlers;
internal class GetMessagesHandler(IDeskproClient deskproClient, IGetPersonHandler getPersonHandler) : IGetMessagesHandler
{
    private readonly IDeskproClient _deskproClient = deskproClient;
    private readonly IGetPersonHandler _getPersonHandler = getPersonHandler;

    public async Task<Result<IReadOnlyCollection<MessageDto>>> Handle(int ticketId, CancellationToken cancellationToken)
    {
        try
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
                        AttachmentIds = x.AttachmentIds,
                        CreatedAt = x.CreatedAt,
                        IsAgentNote = x.IsAgentNote,
                        Content = x.Content,
                        Id = x.Id,
                        Recipients = x.Recipients,
                        CreationSystem = x.CreationSystem,
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

            if (messages is null)
            {
                return Result.Error($"Error getting ticket {ticketId} messages");
            }

            // Add people to the messages 
            foreach (var message in messages)
            {
                var getPersonResult = await _getPersonHandler.Handle(message.Person.Id, cancellationToken);

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
        catch (HttpRequestException ex)
        when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return Result.Error($"Error getting messages from ticket {ticketId}: {ex}");
        }
        catch (Exception)
        {
            throw;
        }
    }
}