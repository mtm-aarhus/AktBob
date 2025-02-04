using AktBob.DatabaseAPI.Contracts.DTOs;
using Ardalis.Result;

namespace AktBob.DatabaseAPI;

public interface IDatabaseApi
{
    Task DeleteMessage(int id, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<MessageDto>>> GetMessageByDeskproMessageId(int deskproMessageId, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<MessageDto>>> GetMessagesNotJournalized(CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<TicketDto>>> GetTicketsByDeskproId(int deskproId, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<TicketDto>>> GetTicketsByPodioItemId(long podioItemId, CancellationToken cancellationToken = default);
    Task<Result<CaseDto>> PostCase(int ticketId, string caseNumber, long? podioItemId, Guid? filArkivCaseId, CancellationToken cancellationToken = default);
    Task<Result<CaseDto>> UpdateCase(int id, long? podioItemId, Guid? filArkivCaseId, CancellationToken cancellationToken = default);
    Task<Result<MessageDto>> UpdateMessage(int id, int? goDocumentId, CancellationToken cancellationToken = default);
}