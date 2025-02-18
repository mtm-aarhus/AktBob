using AktBob.Shared;
using AktBob.Shared.Contracts;
using FastEndpoints;

namespace AktBob.ExternalQueue.Endpoints;
internal class CreateGetOrganizedCaseEndpoint(IJobDispatcher jobDispatcher) : Endpoint<CreateGetOrganizedCaseRequest>
{
    private readonly IJobDispatcher _jobDispatcher = jobDispatcher;

    public override void Configure()
    {
        Post("/Jobs/CreateGetOrganizedCase");
        Options(x => x.WithTags("Jobs"));
    }

    public override async Task HandleAsync(CreateGetOrganizedCaseRequest req, CancellationToken ct)
    {
        var createCaseCommand = new CreateGetOrganizedCaseJob(req.DeskproTicketId, req.CaseTitle);
        _jobDispatcher.Dispatch(createCaseCommand);
        await SendNoContentAsync(ct);
    }
}
