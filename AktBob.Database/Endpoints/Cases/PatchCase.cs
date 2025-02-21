using AktBob.Database.Contracts;
using AktBob.Database.Contracts.Dtos;
using AktBob.Database.Extensions;
using FastEndpoints;
using MassTransit;
using MassTransit.Mediator;
using Microsoft.AspNetCore.Http;

namespace AktBob.Database.Endpoints.Cases;

internal record PatchCaseRequest
{
    public int Id { get; set; }
    public long? PodioItemId { get; set; }
    public string? CaseNumber { get; set; }
    public Guid? FilArkivCaseId { get; set; }
    public string? SharepointFolderName { get; set; }

}

internal class PatchCase(IMediator mediator) : Endpoint<PatchCaseRequest, CaseDto>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Patch("/Database/Cases/{Id}");
        Options(x => x.WithTags("Database/Cases"));

        Description(x => x
            .Produces<CaseDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound));
    }

    public override async Task HandleAsync(PatchCaseRequest req, CancellationToken ct)
    {
        var command = new UpdateCaseCommand(
            Id: req.Id,
            PodioItemId: req.PodioItemId,
            FilArkivCaseId: req.FilArkivCaseId,
            CaseNumber: req.CaseNumber,
            SharepointFolderName: req.SharepointFolderName);

        var result = await _mediator.SendRequest(command, ct);
        await this.SendResponse(result, r => r.Value);
    }
}
