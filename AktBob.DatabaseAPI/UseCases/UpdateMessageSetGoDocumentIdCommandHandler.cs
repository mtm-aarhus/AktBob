using AktBob.DatabaseAPI.Contracts.Commands;
using MassTransit.Mediator;

namespace AktBob.DatabaseAPI.UseCases;
public class UpdateMessageSetGoDocumentIdCommandHandler(IDatabaseApi databaseApi) : MediatorRequestHandler<UpdateMessageSetGoDocumentIdCommand>
{
    private readonly IDatabaseApi _databaseApi = databaseApi;

    protected override async Task Handle(UpdateMessageSetGoDocumentIdCommand request, CancellationToken cancellationToken) => await _databaseApi.UpdateMessage(request.Id, request.GoDocumentId, cancellationToken);
}