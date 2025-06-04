using AAK.Podio.Models;
using AktBob.Podio.Contracts;
using AktBob.Email.Contracts;
using AktBob.Workflows.Helpers;
using AktBob.Deskpro.Contracts;
using AktBob.Shared.Extensions;
using AktBob.Shared.Types.Deskpro;

namespace AktBob.Workflows.Processes.CheckOCRScreeningStatus;

internal class ScreeningIsFinishedEmailNotification : IJobHandler<ScreeningIsFinishedEmailNotificationJob>
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IAppConfig _appConfig;

    private record FieldSection(int AppId, string Label);

    public ScreeningIsFinishedEmailNotification(IServiceScopeFactory serviceScopeFactory, IAppConfig appConfig)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _appConfig = appConfig;
    }

    public async Task Handle(ScreeningIsFinishedEmailNotificationJob job, CancellationToken cancellationToken = default)
    {
        
        
        using var scope = _serviceScopeFactory.CreateScope();
        var podio = scope.ServiceProvider.GetRequiredService<IPodioModule>();
        var deskpro = scope.ServiceProvider.GetRequiredService<IDeskproModule>();
        var email = scope.ServiceProvider.GetRequiredService<IEmailModule>();

        var podioFields = Guard.Against.Null(_appConfig.GetSectionChildren("Podio:Fields").ToDictionary(x => int.Parse(x.Key), x => x.Get<FieldSection>()));
        var sagsansvarligEmailFieldId = podioFields.FirstOrDefault(x => x.Value!.AppId == job.PodioItemId.AppId && x.Value.Label == "SagsansvarligEmail").Key;
        var caseNumberFieldId = podioFields.FirstOrDefault(x => x.Value!.AppId == job.PodioItemId.AppId && x.Value.Label == "CaseNumber").Key;
        var ticketNumberFieldId = podioFields.FirstOrDefault(x => x.Value!.AppId == job.PodioItemId.AppId && x.Value.Label == "DeskproId").Key;

        // Get Podio Items
        var podioItemResult = await podio.GetItem(job.PodioItemId, cancellationToken);
        if (podioItemResult.IsError)
        {
            throw new BusinessException(podioItemResult.Errors.ToCommaDelimitedString());
        }

        var to = podioItemResult.Value.Fields.FirstOrDefault(f => f.Id == sagsansvarligEmailFieldId)?.GetValues<FieldValueText>()?.Value ?? throw new BusinessException($"Not able to get recipient email from {job.PodioItemId} field {sagsansvarligEmailFieldId}");
        var caseNumber = podioItemResult.Value.Fields.FirstOrDefault(f => f.Id == caseNumberFieldId)?.GetValues<FieldValueText>()?.Value ?? "IKKE ANGIVET";
        var ticketId = podioItemResult.Value.Fields.FirstOrDefault(f => f.Id == ticketNumberFieldId)?.GetValues<FieldValueText>()?.Value ?? "IKKE ANGIVET";
        var ticketSubject = string.Empty;

        // Try get Deskpro ticket
        if (TicketId.TryParse(ticketId, default, out var deskproTicketId))
        {
            var getDeskproTicket = await deskpro.GetTicket(deskproTicketId, cancellationToken);
            if (!getDeskproTicket.IsError)
            {
                ticketSubject = getDeskproTicket.Value.Subject;
            }
        }

        // Notify
        var subject = $"{caseNumber}: Screening færdig";
        var fields = new Dictionary<string, string>
        {
            { "caseNumber", caseNumber },
            { "ticketSubject", ticketSubject },
            { "ticketId", ticketId },
            { "linkFilArkivCase", $"https://aarhus.filarkiv.dk/archives/case/{job.FilArkivCaseId}" }
        };

        var emailBody = HtmlHelper.GenerateHtml(fields, "EmailTemplates/ocr-screening-finished.html");
        email.Send(to, subject, emailBody, true);
    }
}
