using AktBob.DatabaseAPI.Contracts.DTOs;
using AktBob.DatabaseAPI.Contracts.Queries;
using Ardalis.Result;
using MassTransit.Mediator;

namespace AktBob.DatabaseAPI.UseCases;
internal class GetTicketByPodioItemIdQueryHandler(IDatabaseApi databaseApi) : MediatorRequestHandler<GetTicketByPodioItemIdQuery, Result<IEnumerable<TicketDto>>>
{
    private readonly IDatabaseApi _databaseApi = databaseApi;

    protected override async Task<Result<IEnumerable<TicketDto>>> Handle(GetTicketByPodioItemIdQuery request, CancellationToken cancellationToken) => await _databaseApi.GetTicketsByPodioItemId(request.PodioItemId, cancellationToken);
}
