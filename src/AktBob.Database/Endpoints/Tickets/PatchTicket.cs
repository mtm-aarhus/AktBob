using AktBob.Database.Dtos;
using AktBob.Shared;
using AktBob.Shared.Contracts.Database;
using AktBob.Shared.Jobs;
using FastEndpoints;
using Microsoft.AspNetCore.Http;

namespace AktBob.Database.Endpoints.Tickets;

internal record PatchTicketRequest
{
    public int Id { get; set; }
    public string? CaseNumber { get; set; }
    public string? CaseUrl { get; set; }
    public string? SharepointFolderName { get; set; }
}

internal class PatchTicket(IJobDispatcher jobDispatcher) : Endpoint<PatchTicketRequest, TicketDto>
{
    private readonly IJobDispatcher _jobDispatcher = jobDispatcher;

    public override void Configure()
    {
        Patch("/Database/Tickets/{Id}");
        Options(x => x.WithTags("Database/Tickets"));

        Description(x => x.Produces<TicketDto>(StatusCodes.Status204NoContent));
    }

    public override async Task HandleAsync(PatchTicketRequest req, CancellationToken ct)
    {
        var job = new UpdateDatabaseTicketJob(req.Id, req.CaseNumber, req.CaseUrl, req.SharepointFolderName);
        _jobDispatcher.Dispatch(job);
        await SendNoContentAsync(ct);
    }
}
