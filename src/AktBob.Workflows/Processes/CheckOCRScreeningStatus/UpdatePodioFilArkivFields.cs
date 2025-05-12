using AktBob.Podio.Contracts;

namespace AktBob.Workflows.Processes.CheckOCRScreeningStatus;
internal class UpdatePodioFilArkivFields : IJobHandler<UpdatePodioFilArkivFieldsJob>
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IConfiguration _configuration;

    record FieldSection(int AppId, string Label);

    public UpdatePodioFilArkivFields(IServiceScopeFactory serviceScopeFactory, IConfiguration configuration)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _configuration = configuration;
    }

    public Task Handle(UpdatePodioFilArkivFieldsJob job, CancellationToken cancellationToken = default)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var podio = scope.ServiceProvider.GetRequiredService<IPodioModule>();

        var podioAppId = Guard.Against.Null(_configuration.GetValue<int>("Podio:AppId"));
        var podioFields = Guard.Against.Null(_configuration.GetSection("Podio:Fields").GetChildren().ToDictionary(x => int.Parse(x.Key), x => x.Get<FieldSection>()));

        // FilArkivCaseId
        var filArkivCaseIdFieldId = podioFields.FirstOrDefault(x => x.Value!.AppId == podioAppId && x.Value.Label == "FilArkivCaseId").Key;
        podio.UpdateTextField(new UpdateTextFieldCommand(job.PodioItemId, filArkivCaseIdFieldId, job.FilArkivCaseId.ToString()));

        // FilArkivLink
        var filArkivLinkFieldId = podioFields.FirstOrDefault(x => x.Value!.AppId == podioAppId && x.Value.Label == "FilArkivLink").Key;
        podio.UpdateTextField(new UpdateTextFieldCommand(job.PodioItemId, filArkivLinkFieldId, $"https://aarhus.filarkiv.dk/archives/case/{job.FilArkivCaseId}"));

        return Task.CompletedTask;
    }
}
