using AktBob.DatabaseAPI.Contracts.DTOs;
using AktBob.DatabaseAPI.Contracts.Queries;
using Ardalis.Result;
using MediatR;

namespace AktBob.DatabaseAPI.UseCases;
internal class GetTicketByDeskproIdQueryHandler : IRequestHandler<GetTicketByDeskproIdQuery, Result<IEnumerable<TicketDto>>>
{
    private readonly IDatabaseApi _databaseApi;

    public GetTicketByDeskproIdQueryHandler(IDatabaseApi databaseApi)
    {
        _databaseApi = databaseApi;
    }

    public async Task<Result<IEnumerable<TicketDto>>> Handle(GetTicketByDeskproIdQuery request, CancellationToken cancellationToken) => await _databaseApi.GetTicketsByDeskproId(request.DeskproId, cancellationToken);
}
