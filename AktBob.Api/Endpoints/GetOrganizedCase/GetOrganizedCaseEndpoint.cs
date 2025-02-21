using AktBob.Shared;
using AktBob.Shared.Contracts;
using FastEndpoints;

namespace AktBob.Api.Endpoints.GetOrganizedCase;
internal class GetOrganizedCaseEndpoint(IJobDispatcher jobDispatcher) : Endpoint<GetOrganizedCaseRequest>
{
    private readonly IJobDispatcher _jobDispatcher = jobDispatcher;

    public override void Configure()
    {
        Post("/Jobs/GetOrganizedCase");
        Options(x => x.WithTags("Jobs"));
        Summary(s =>
        {
            s.Summary = "Creates a new GetOrganized case and updates the Deskpro ticket with the GetOrganized case ID/URL";
        });
    }

    public override async Task HandleAsync(GetOrganizedCaseRequest req, CancellationToken ct)
    {
        var createCaseCommand = new CreateGetOrganizedCaseJob(req.DeskproTicketId, req.CaseTitle);
        _jobDispatcher.Dispatch(createCaseCommand);
        await SendNoContentAsync(ct);
    }
}
