using AktBob.DatabaseAPI.Contracts.Commands;
using AktBob.DatabaseAPI.Contracts.DTOs;
using Ardalis.Result;
using MassTransit.Mediator;
using Microsoft.Extensions.Logging;

namespace AktBob.DatabaseAPI.UseCases;
public class UpdateCaseSetFilArkivCaseIdCommandHandler(IDatabaseApi databaseApi, ILogger<UpdateCaseSetFilArkivCaseIdCommandHandler> logger) : MediatorRequestHandler<UpdateCaseSetFilArkivCaseIdCommand, Result<CaseDto>>
{
    private readonly IDatabaseApi _databaseApi = databaseApi;
    private readonly ILogger<UpdateCaseSetFilArkivCaseIdCommandHandler> _logger = logger;

    protected override async Task<Result<CaseDto>> Handle(UpdateCaseSetFilArkivCaseIdCommand request, CancellationToken cancellationToken)
    {
        var tickets = await _databaseApi.GetTicketsByPodioItemId(request.PodioItemId, cancellationToken);

        if (!tickets.IsSuccess || tickets.Value.Count() == 0)
        {
            return Result.Error();
        }

        if (tickets.Value.Count() > 1)
        {
            _logger.LogError("{count} tickets registered in database with a case with Podio item id '{id}'. Database will not be updated.", tickets.Value.Count(), request.PodioItemId);
            return Result.Error();
        }

        var cases = tickets.Value.SelectMany(t => t.Cases).Where(c => c.PodioItemId ==  request.PodioItemId);

        if (cases.Count() > 1)
        {
            _logger.LogError("{count} cases registered in database with Podio item id '{id}'. Database will not be updated.", tickets.Value.Count(), request.PodioItemId);
            return Result.Error();
        }

        if (cases.Count() == 0)
        {
            _logger.LogError("0 cases registered in database with Podio item id '{id}'. Database will not be updated.", request.PodioItemId);
            return Result.Error();
        }

        var @case = cases.First();

        return await _databaseApi.UpdateCase(@case.Id, podioItemId: null, filArkivCaseId: request.FilArkivCaseId, cancellationToken);
    }
}
