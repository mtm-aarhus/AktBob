using AktBob.DatabaseAPI.Contracts;
using Ardalis.Result;

namespace AktBob.DatabaseAPI;

internal interface IDatabaseApi
{
    Task<Result<IEnumerable<TicketDto>>> GetTicketByPodioItemId(long podioItemId, CancellationToken cancellationToken = default);
}