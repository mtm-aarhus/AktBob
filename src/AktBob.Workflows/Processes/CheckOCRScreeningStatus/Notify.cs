using AAK.Podio.Models;
using AktBob.Email.Contracts;
using AktBob.Podio.Contracts;

namespace AktBob.Workflows.Processes.CheckOCRScreeningStatus;
internal static class Notify
{
    record FieldSection(int AppId, string Label);

    public static async Task ScreeningIsFinished(IPodioModule podio, IEmailModule email, IAppConfig appConfig, PodioItemId podioItemId, Guid filArkivCaseId, CancellationToken cancellationToken)
    {
        var podioFields = Guard.Against.Null(appConfig.GetSectionChildren("Podio:Fields").ToDictionary(x => int.Parse(x.Key), x => x.Get<FieldSection>()));
        var sagsansvarligEmailFieldId = podioFields.FirstOrDefault(x => x.Value!.AppId == podioItemId.AppId && x.Value.Label == "SagsansvarligEmail").Key;
        var caseNumberFieldId = podioFields.FirstOrDefault(x => x.Value!.AppId == podioItemId.AppId && x.Value.Label == "CaseNumber").Key;

        var podioItemResult = await podio.GetItem(podioItemId, cancellationToken);
        if (!podioItemResult.IsSuccess || podioItemResult.Value is null)
        {
            throw new BusinessException($"Could not get {podioItemId} from Podio");
        }

        var to = podioItemResult.Value.Fields.FirstOrDefault(f => f.Id == sagsansvarligEmailFieldId)?.GetValues<FieldValueText>()?.Value ?? throw new BusinessException($"Not able to get recipient email from {podioItemId} field {sagsansvarligEmailFieldId}");
        var caseNumber = podioItemResult.Value.Fields.FirstOrDefault(f => f.Id == caseNumberFieldId)?.GetValues<FieldValueText>()?.Value ?? "IKKE ANGIVET";

        var subject = $"OCR screening af dokumenterne på sag {caseNumber} er færdig";
        var filArkivLink = $"https://aarhus.filarkiv.dk/archives/case/{filArkivCaseId}";
        var body = $"""
            <h1>OCR-screening af dokumenterne på sag {caseNumber} er færdig</h1>
            <p>Link: <a href="{filArkivLink}">{filArkivLink}</a></p>
            """;
        email.Send(to, subject, body, true);
    }
}
