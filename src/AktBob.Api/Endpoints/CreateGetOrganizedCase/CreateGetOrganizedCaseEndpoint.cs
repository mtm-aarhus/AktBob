using AktBob.Shared;
using AktBob.Shared.Jobs;
using FastEndpoints;

namespace AktBob.Api.Endpoints.GetOrganizedCase;
internal class CreateGetOrganizedCaseEndpoint(IJobDispatcher jobDispatcher) : Endpoint<CreateGetOrganizedCaseRequest>
{
    private readonly IJobDispatcher _jobDispatcher = jobDispatcher;

    public override void Configure()
    {
        Post("/Jobs/CreateGetOrganizedCase");
        Options(x => x.WithTags("Jobs"));
        Summary(s =>
        {
            s.Summary = "Creates a new GetOrganized case and updates the Deskpro ticket with the GetOrganized case ID/URL";
        });
    }

    public override async Task HandleAsync(CreateGetOrganizedCaseRequest req, CancellationToken ct)
    {
        _jobDispatcher.Dispatch(new CreateGetOrganizedCaseJob(req.DeskproTicketId));
        await SendNoContentAsync(ct);
    }
}
