using AktBob.Shared;
using AktBob.Shared.Jobs;
using FastEndpoints;

namespace AktBob.Api.Endpoints.UpdateDeskproSetGetOrganizedAggregatedCases;

internal class UpdateDeskproSetGetOrganizedAggregatedCaseNumbersEndpoint(IJobDispatcher jobDispatcher) : Endpoint<UpdateDeskproSetGetOrganizedAggregatedCaseNumbersRequest, string[]>
{
    private readonly IJobDispatcher _jobDispatcher = jobDispatcher;

    public override void Configure()
    {
        Post("/Jobs/UpdateDeskproSetGetAggregatedCaseNumbers");
        Options(x => x.WithTags("Jobs"));
    }

    public override async Task HandleAsync(UpdateDeskproSetGetOrganizedAggregatedCaseNumbersRequest req, CancellationToken ct)
    {
        var splittedCaseIds = req.CaseIds.Split(",");
        _jobDispatcher.Dispatch(new UpdateDeskproSetGetOrganizedAggregatedCaseNumbersJob(splittedCaseIds, req.DeskproTicketId));
        await SendNoContentAsync(ct);
    }
}
