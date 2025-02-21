using AktBob.Podio.Contracts;

namespace AktBob.JobHandlers.Handlers.CheckOCRScreeningStatus;
internal class UpdatePodioItemJob
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<UpdatePodioItemJob> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public UpdatePodioItemJob(IConfiguration configuration, ILogger<UpdatePodioItemJob> logger, IServiceScopeFactory serviceScopeFactory)
    {
        _configuration = configuration;
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task Run(Guid filArkivCaseId, long podioItemId, CancellationToken cancellationToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var podioAppId = Guard.Against.Null(_configuration.GetValue<int>("Podio:AppId"));
        var podioFields = Guard.Against.Null(_configuration.GetSection("Podio:Fields").GetChildren().ToDictionary(x => int.Parse(x.Key), x => x.Get<FieldSection>()));

        // FilArkivCaseId
        var filArkivCaseIdFieldId = podioFields.FirstOrDefault(x => x.Value.AppId == podioAppId && x.Value.Label == "FilArkivCaseId").Key;
        await UpdateField(mediator, podioItemId, podioAppId, filArkivCaseIdFieldId, filArkivCaseId.ToString(), cancellationToken);

        // FilArkivLink
        var filArkivLinkFieldId = podioFields.FirstOrDefault(x => x.Value.AppId == podioAppId && x.Value.Label == "FilArkivLink").Key;
        await UpdateField(mediator, podioItemId, podioAppId, filArkivLinkFieldId, $"https://aarhus.filarkiv.dk/archives/case/{filArkivCaseId}", cancellationToken);
    }

    private async Task UpdateField(IMediator mediator, long podioItemId, int podioAppId, int fieldId, string value, CancellationToken cancellationToken)
    {
        var updateFilArkivCaseIdFieldCommand = new UpdateFieldCommand(podioAppId, podioItemId, fieldId, value);
        await mediator.Send(updateFilArkivCaseIdFieldCommand, cancellationToken);
    }

    record FieldSection(int AppId, string Label);
}
