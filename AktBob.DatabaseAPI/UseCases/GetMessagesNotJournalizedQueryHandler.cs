using AktBob.DatabaseAPI.Contracts.DTOs;
using AktBob.DatabaseAPI.Contracts.Queries;
using Ardalis.Result;
using MediatR;

namespace AktBob.DatabaseAPI.UseCases;
internal class GetMessagesNotJournalizedQueryHandler : IRequestHandler<GetMessagesNotJournalizedQuery, Result<IEnumerable<MessageDto>>>
{
    private readonly IDatabaseApi _databaseApi;

    public GetMessagesNotJournalizedQueryHandler(IDatabaseApi databaseApi)
    {
        _databaseApi = databaseApi;
    }
    public async Task<Result<IEnumerable<MessageDto>>> Handle(GetMessagesNotJournalizedQuery request, CancellationToken cancellationToken) => await _databaseApi.GetMessagesNotJournalized();
}
