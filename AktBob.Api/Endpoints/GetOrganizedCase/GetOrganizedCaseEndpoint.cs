using AktBob.Shared;
using AktBob.Shared.Contracts;
using FastEndpoints;

namespace AktBob.Api.Endpoints.GetOrganizedCase;
internal class GetOrganizedCaseEndpoint(IJobDispatcher jobDispatcher) : Endpoint<GetOrganizedCaseRequest>
{
    private readonly IJobDispatcher _jobDispatcher = jobDispatcher;

    public override void Configure()
    {
        Post("/Jobs/GetOrganizedCase", "/Jobs/CreateGetOrganizedCase");
        Options(x => x.WithTags("Jobs"));
    }

    public override async Task HandleAsync(GetOrganizedCaseRequest req, CancellationToken ct)
    {
        var createCaseCommand = new CreateGetOrganizedCaseJob(req.DeskproTicketId, req.CaseTitle);
        _jobDispatcher.Dispatch(createCaseCommand);
        await SendNoContentAsync(ct);
    }
}
