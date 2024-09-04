using AktBob.DatabaseAPI.Contracts;
using Ardalis.Result;
using MediatR;

namespace AktBob.DatabaseAPI.UseCases;

internal class PostCaseCommandHandler : IRequestHandler<PostCaseCommand, Result<CaseDto>>
{
    private readonly IDatabaseApi _databaseApi;

    public PostCaseCommandHandler(IDatabaseApi databaseApi)
    {
        _databaseApi = databaseApi;
    }

    public async Task<Result<CaseDto>> Handle(PostCaseCommand request, CancellationToken cancellationToken) => await _databaseApi.PostCase(request.TicketId, request.PodioItemId, request.FilArkivCaseId, cancellationToken);
}
