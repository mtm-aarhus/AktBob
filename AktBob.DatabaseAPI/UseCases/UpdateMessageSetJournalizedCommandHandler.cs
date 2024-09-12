using AktBob.DatabaseAPI.Contracts.Commands;
using MediatR;

namespace AktBob.DatabaseAPI.UseCases;
internal class UpdateMessageSetJournalizedCommandHandler : IRequestHandler<UpdateMessageSetJournalizedCommand>
{
    private readonly IDatabaseApi _databaseApi;

    public UpdateMessageSetJournalizedCommandHandler(IDatabaseApi databaseApi)
    {
        _databaseApi = databaseApi;
    }

    public async Task Handle(UpdateMessageSetJournalizedCommand request, CancellationToken cancellationToken) => await _databaseApi.UpdateMessage(request.Id, request.JournalizedAt, request.GoDocumentId, cancellationToken);
}