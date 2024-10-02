using AktBob.DatabaseAPI.Contracts.Commands;
using MediatR;

namespace AktBob.DatabaseAPI.UseCases;
internal class DeleteMessageCommandHandler : IRequestHandler<DeleteMessageCommand>
{
    private readonly IDatabaseApi _databaseApi;

    public DeleteMessageCommandHandler(IDatabaseApi databaseApi)
    {
        _databaseApi = databaseApi;
    }

    public async Task Handle(DeleteMessageCommand request, CancellationToken cancellationToken) => await _databaseApi.DeleteMessage(request.Id, cancellationToken);
}
