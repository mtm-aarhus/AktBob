using AktBob.DatabaseAPI.Contracts;
using Ardalis.Result;

namespace AktBob.DatabaseAPI;

internal interface IDatabaseApi
{
    Task<Result<IEnumerable<TicketDto>>> GetTicketsByDeskproId(int deskproId, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<TicketDto>>> GetTicketsByPodioItemId(long podioItemId, CancellationToken cancellationToken = default);
    Task<Result<CaseDto>> PostCase(int ticketId, string caseNumber, long? podioItemId, Guid? filArkivCaseId, CancellationToken cancellationToken = default);
    Task<Result<CaseDto>> UpdateCase(int id, long? podioItemId, Guid? filArkivCaseId, CancellationToken cancellationToken = default);
}