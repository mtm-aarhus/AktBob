using AktBob.DatabaseAPI.Contracts.Commands;
using AktBob.DatabaseAPI.Contracts.DTOs;
using Ardalis.Result;
using MassTransit.Mediator;

namespace AktBob.DatabaseAPI.UseCases;

public class PostCaseCommandHandler(IDatabaseApi databaseApi) : MediatorRequestHandler<PostCaseCommand, Result<CaseDto>>
{
    private readonly IDatabaseApi _databaseApi = databaseApi;

    protected override async Task<Result<CaseDto>> Handle(PostCaseCommand request, CancellationToken cancellationToken) => await _databaseApi.PostCase(request.TicketId, request.CaseNumber, request.PodioItemId, request.FilArkivCaseId, cancellationToken);
}
