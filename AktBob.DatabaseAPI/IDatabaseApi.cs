using AktBob.DatabaseAPI.Contracts;
using Ardalis.Result;

namespace AktBob.DatabaseAPI;

internal interface IDatabaseApi
{
    Task<Result<IEnumerable<TicketDto>>> GetTicketByPodioItemId(long podioItemId, CancellationToken cancellationToken = default);
    Task<Result<CaseDto>> PostCase(int ticketId, long? podioItemId, Guid? filArkivCaseId, CancellationToken cancellationToken = default);
    Task<Result<CaseDto>> UpdateCase(int id, long? podioItemId, Guid? filArkivCaseId, CancellationToken cancellationToken = default);
}