using AktBob.DatabaseAPI.Contracts.Commands;
using MassTransit.Mediator;

namespace AktBob.DatabaseAPI.UseCases;
internal class DeleteMessageCommandHandler : MediatorRequestHandler<DeleteMessageCommand>
{
    private readonly IDatabaseApi _databaseApi;

    public DeleteMessageCommandHandler(IDatabaseApi databaseApi)
    {
        _databaseApi = databaseApi;
    }

    protected override async Task Handle(DeleteMessageCommand request, CancellationToken cancellationToken) => await _databaseApi.DeleteMessage(request.Id, cancellationToken);
}
