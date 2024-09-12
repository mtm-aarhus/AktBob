using AktBob.DatabaseAPI.Contracts.Commands;
using MediatR;

namespace AktBob.DatabaseAPI.UseCases;
internal class UpdateMessageSetGoDocumentIdCommandHandler : IRequestHandler<UpdateMessageSetGoDocumentIdCommand>
{
    private readonly IDatabaseApi _databaseApi;

    public UpdateMessageSetGoDocumentIdCommandHandler(IDatabaseApi databaseApi)
    {
        _databaseApi = databaseApi;
    }

    public async Task Handle(UpdateMessageSetGoDocumentIdCommand request, CancellationToken cancellationToken) => await _databaseApi.UpdateMessage(request.Id, request.GoDocumentId, cancellationToken);
}