using AktBob.Podio.Contracts;

namespace AktBob.Workflows.Processes.CheckOCRScreeningStatus;
internal static class UpdatePodioField
{
    record FieldSection(int AppId, string Label);

    public static void SetFilArkivCaseId(IPodioModule podio, IConfiguration configuration, Guid filArkivCaseId, PodioItemId podioItemId)
    {
        var podioAppId = Guard.Against.Null(configuration.GetValue<int>("Podio:AppId"));
        var podioFields = Guard.Against.Null(configuration.GetSection("Podio:Fields").GetChildren().ToDictionary(x => int.Parse(x.Key), x => x.Get<FieldSection>()));

        // FilArkivCaseId
        var filArkivCaseIdFieldId = podioFields.FirstOrDefault(x => x.Value!.AppId == podioAppId && x.Value.Label == "FilArkivCaseId").Key;
        podio.UpdateTextField(new UpdateTextFieldCommand(podioItemId, filArkivCaseIdFieldId, filArkivCaseId.ToString()));

        // FilArkivLink
        var filArkivLinkFieldId = podioFields.FirstOrDefault(x => x.Value!.AppId == podioAppId && x.Value.Label == "FilArkivLink").Key;
        podio.UpdateTextField(new UpdateTextFieldCommand(podioItemId, filArkivLinkFieldId, $"https://aarhus.filarkiv.dk/archives/case/{filArkivCaseId}"));
    }
}
