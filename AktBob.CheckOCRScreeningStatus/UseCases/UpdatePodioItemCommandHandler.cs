using AktBob.Podio.Contracts;

namespace AktBob.CheckOCRScreeningStatus.UseCases;
public record UpdatePodioItemCommand(Guid FilArkivCaseId);

public class UpdatePodioItemCommandHandler : MediatorRequestHandler<UpdatePodioItemCommand>
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

    protected override async Task Handle(UpdatePodioItemCommand command, CancellationToken cancellationToken)
    {
        var @case = _data.GetCase(command.FilArkivCaseId);

        if (@case == null)
        {
            _logger.LogError("Case {id} not found in cache", command.FilArkivCaseId);
            return;
        }

        _logger.LogInformation("Updating Podio item {id}, setting FilArkivCaseId {filarkivCaseId}", @case.PodioItemId, command.FilArkivCaseId);

        var podioAppId = Convert.ToInt32(_configuration.GetValue<int?>("Podio:AppId"));
        var podioFields = _configuration.GetSection("Podio:Fields").GetChildren().ToDictionary(x => int.Parse(x.Key), x => x.Get<PodioField>());
        var podioFieldFilArkivCaseId = podioFields.FirstOrDefault(x => x.Value.AppId == podioAppId && x.Value.Label == "FilArkivCaseId");
        var podioFieldFilArkivLink = podioFields.FirstOrDefault(x => x.Value.AppId == podioAppId && x.Value.Label == "FilArkivLink");

        var updateFilArkivCaseIdFieldCommand = new UpdateFieldCommand(podioAppId, @case.PodioItemId, podioFieldFilArkivCaseId.Key, command.FilArkivCaseId.ToString());
        await _mediator.Send(updateFilArkivCaseIdFieldCommand, cancellationToken);

        var updateFilArkivLinkFieldCommand = new UpdateFieldCommand(podioAppId, @case.PodioItemId, podioFieldFilArkivLink.Key, $"https://aarhus.filarkiv.dk/archives/case/{command.FilArkivCaseId}");
        await _mediator.Send(updateFilArkivLinkFieldCommand, cancellationToken);

        return;
    }
}
