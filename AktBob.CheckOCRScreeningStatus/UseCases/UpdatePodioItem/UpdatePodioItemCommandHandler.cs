using AktBob.Podio.Contracts;
using Ardalis.Result;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AktBob.CheckOCRScreeningStatus.UseCases.UpdatePodioItem;
internal class UpdatePodioItemCommandHandler : IRequestHandler<UpdatePodioItemCommand, Result>
{
    private readonly IData _data;
    private readonly IConfiguration _configuration;
    private readonly IMediator _mediator;
    private readonly ILogger<UpdatePodioItemCommandHandler> _logger;

    public UpdatePodioItemCommandHandler(IData data, IConfiguration configuration, IMediator mediator, ILogger<UpdatePodioItemCommandHandler> logger)
    {
        _data = data;
        _configuration = configuration;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<Result> Handle(UpdatePodioItemCommand request, CancellationToken cancellationToken)
    {
        var @case = _data.GetCase(request.FilArkivCaseId);

        if (@case == null)
        {
            return Result.Error(new ErrorList([$"Case {request.FilArkivCaseId} not found"], string.Empty));
        }

        if (@case.PodioItemUpdated)
        {
            return Result.SuccessWithMessage($"Case {request.FilArkivCaseId}: PodioItem already updated");
        }

        var podioAppId = Convert.ToInt32(_configuration.GetValue<int?>("Podio:AppId"));
        var podioFields = _configuration.GetSection("Podio:Fields").GetChildren().ToDictionary(x => int.Parse(x.Key), x => x.Get<PodioField>());
        var podioFieldFilArkivCaseId = podioFields.FirstOrDefault(x => x.Value.AppId == podioAppId && x.Value.Label == "FilArkivCaseId");
        var podioFieldFilArkivLink = podioFields.FirstOrDefault(x => x.Value.AppId == podioAppId && x.Value.Label == "FilArkivLink");

        var updateFilArkivCaseIdFieldCommand = new UpdateItemFieldCommand(podioAppId, @case.PodioItemId, podioFieldFilArkivCaseId.Key, @case.CaseId.ToString());
        var updateFilArkivCaseIdFieldCommandResult = await _mediator.Send(updateFilArkivCaseIdFieldCommand, cancellationToken);

        if (!updateFilArkivCaseIdFieldCommandResult.IsSuccess)
        {
            return Result.Error();
        }

        var updateFilArkivLinkFieldCommand = new UpdateItemFieldCommand(podioAppId, @case.PodioItemId, podioFieldFilArkivLink.Key, $"https://aarhus.filarkiv.dk/archives/case/{@case.CaseId.ToString()}");
        var updateFilArkivLinkFieldCommandResult = await _mediator.Send(updateFilArkivLinkFieldCommand, cancellationToken);

        if (!updateFilArkivLinkFieldCommandResult.IsSuccess)
        {
            return Result.Error();
        }

        return Result.Success();
    }
}
