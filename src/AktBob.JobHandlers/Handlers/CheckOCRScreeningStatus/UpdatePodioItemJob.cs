using AktBob.Podio.Contracts;

namespace AktBob.JobHandlers.Handlers.CheckOCRScreeningStatus;
internal class UpdatePodioItemJob
{
    private readonly IConfiguration _configuration;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public UpdatePodioItemJob(IConfiguration configuration, IServiceScopeFactory serviceScopeFactory)
    {
        _configuration = configuration;
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task Run(Guid filArkivCaseId, long podioItemId, CancellationToken cancellationToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var updatePodioFieldHandler = scope.ServiceProvider.GetRequiredService<IUpdatePodioFieldHandler>();

        var podioAppId = Guard.Against.Null(_configuration.GetValue<int>("Podio:AppId"));
        var podioFields = Guard.Against.Null(_configuration.GetSection("Podio:Fields").GetChildren().ToDictionary(x => int.Parse(x.Key), x => x.Get<FieldSection>()));

        // FilArkivCaseId
        var filArkivCaseIdFieldId = podioFields.FirstOrDefault(x => x.Value!.AppId == podioAppId && x.Value.Label == "FilArkivCaseId").Key;
        await updatePodioFieldHandler.Handle(podioAppId, podioItemId, filArkivCaseIdFieldId, filArkivCaseId.ToString(), cancellationToken);

        // FilArkivLink
        var filArkivLinkFieldId = podioFields.FirstOrDefault(x => x.Value!.AppId == podioAppId && x.Value.Label == "FilArkivLink").Key;
        await updatePodioFieldHandler.Handle(podioAppId, podioItemId, filArkivLinkFieldId, $"https://aarhus.filarkiv.dk/archives/case/{filArkivCaseId}", cancellationToken);
    }

    record FieldSection(int AppId, string Label);
}
