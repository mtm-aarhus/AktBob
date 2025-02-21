using AktBob.Shared;
using AktBob.Shared.Contracts;
using FastEndpoints;

namespace AktBob.Api.Endpoints.DeskproTicketToGetOrganized;

internal class DeskproTicketToGetOrganizedEndpoint(IJobDispatcher jobDispatcher) : Endpoint<DeskproTicketToGetOrganizedRequest>
{
    private readonly IJobDispatcher _jobDispatcher = jobDispatcher;

    public override void Configure()
    {
        Post("/Jobs/DeskproTicketToGetOrganized");
        Options(x => x.WithTags("Jobs"));
        Summary(s =>
        {
            s.Summary = "Adds or updates a PDF version of the Deskpro ticket including messages to the case in GetOrganized";
        });
    }

    public override async Task HandleAsync(DeskproTicketToGetOrganizedRequest req, CancellationToken ct)
    {
        var command = new AddOrUpdateDeskproTicketToGetOrganizedJob
        {
            TicketId = req.TicketId,
            GOCaseNumber = req.GOCaseNumber,
            CustomFieldIds = req.CustomFieldIds,
            CaseNumberFieldIds = req.CaseNumberFieldIds
        };

        _jobDispatcher.Dispatch(command);
        await SendNoContentAsync(ct);
    }
}
