using AktBob.DatabaseAPI.Contracts.Commands;
using AktBob.DatabaseAPI.Contracts.DTOs;
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

    public async Task<Result<CaseDto>> Handle(PostCaseCommand request, CancellationToken cancellationToken) => await _databaseApi.PostCase(request.TicketId, request.CaseNumber, request.PodioItemId, request.FilArkivCaseId, cancellationToken);
}
