using AktBob.Shared.Contracts.Modules.Deskpro.DTOs;

namespace Aktbob.Modules.Deskpro.Features.GetTicketsByFieldSearch;
internal interface IGetTicketsByFieldSearchHandler
{
    Task<ErrorOr<IReadOnlyCollection<TicketDto>>> Handle(int[] fields, string searchValue, CancellationToken cancellationToken);
}