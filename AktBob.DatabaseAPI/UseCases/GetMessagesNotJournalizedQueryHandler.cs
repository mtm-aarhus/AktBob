using AktBob.DatabaseAPI.Contracts.DTOs;
using AktBob.DatabaseAPI.Contracts.Queries;
using Ardalis.Result;
using MassTransit.Mediator;

namespace AktBob.DatabaseAPI.UseCases;
public class GetMessagesNotJournalizedQueryHandler(IDatabaseApi databaseApi) : MediatorRequestHandler<GetMessagesNotJournalizedQuery, Result<IEnumerable<MessageDto>>>
{
    private readonly IDatabaseApi _databaseApi = databaseApi;

    protected override async Task<Result<IEnumerable<MessageDto>>> Handle(GetMessagesNotJournalizedQuery request, CancellationToken cancellationToken) => await _databaseApi.GetMessagesNotJournalized();
}
