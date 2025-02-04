using AktBob.DatabaseAPI.Contracts.DTOs;
using AktBob.DatabaseAPI.Contracts.Queries;
using Ardalis.Result;
using MassTransit.Mediator;

namespace AktBob.DatabaseAPI.UseCases;
internal class GetTicketByDeskproIdQueryHandler(IDatabaseApi databaseApi) : MediatorRequestHandler<GetTicketByDeskproIdQuery, Result<IEnumerable<TicketDto>>>
{
    private readonly IDatabaseApi _databaseApi = databaseApi;

    protected override async Task<Result<IEnumerable<TicketDto>>> Handle(GetTicketByDeskproIdQuery request, CancellationToken cancellationToken) => await _databaseApi.GetTicketsByDeskproId(request.DeskproId, cancellationToken);
}
