using AktBob.DatabaseAPI.Contracts;
using Ardalis.Result;
using MediatR;

namespace AktBob.DatabaseAPI.UseCases;
internal class GetTicketByPodioItemIdQueryHandler : IRequestHandler<GetTicketByPodioItemIdQuery, Result<IEnumerable<TicketDto>>>
{
    private readonly IDatabaseApi _databaseApi;

    public GetTicketByPodioItemIdQueryHandler(IDatabaseApi databaseApi)
    {
        _databaseApi = databaseApi;
    }

    public async Task<Result<IEnumerable<TicketDto>>> Handle(GetTicketByPodioItemIdQuery request, CancellationToken cancellationToken) => await _databaseApi.GetTicketsByPodioItemId(request.PodioItemId, cancellationToken);
}
