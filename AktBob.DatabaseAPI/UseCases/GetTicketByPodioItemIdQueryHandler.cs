using AktBob.DatabaseAPI.Contracts.DTOs;
using AktBob.DatabaseAPI.Contracts.Queries;
using Ardalis.Result;
using MediatR;

namespace AktBob.DatabaseAPI.UseCases;
internal class GetTicketByPodioItemIdQueryHandler(IDatabaseApi databaseApi) : IRequestHandler<GetTicketByPodioItemIdQuery, Result<IEnumerable<TicketDto>>>
{
    private readonly IDatabaseApi _databaseApi = databaseApi;
    public async Task<Result<IEnumerable<TicketDto>>> Handle(GetTicketByPodioItemIdQuery request, CancellationToken cancellationToken) => await _databaseApi.GetTicketsByPodioItemId(request.PodioItemId, cancellationToken);
}
