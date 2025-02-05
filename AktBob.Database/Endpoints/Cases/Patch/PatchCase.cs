using AktBob.Database.Contracts.Dtos;
using AktBob.Database.Extensions;
using AktBob.Database.UseCases.Cases.PatchCase;
using FastEndpoints;
using MassTransit;
using MassTransit.Mediator;
using Microsoft.AspNetCore.Http;

namespace AktBob.Database.Endpoints.Cases.Patch;
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

        Summary(x =>
        {
            x.Description = "Opdaterer de angivne felter for en specific case i databasen. Alle felter er valgfrie og ignoreres hvis de enten ikke angives eller angives til null. Returnerer den opdaterede case.";
        });
    }

    public override async Task HandleAsync(PatchCaseRequest req, CancellationToken ct)
    {
        var command = new PatchCaseCommand(
            Id: req.Id,
            PodioItemId: req.PodioItemId,
            FilArkivCaseId: req.FilArkivCaseId,
            CaseNumber: req.CaseNumber,
            SharepointFolderName: req.SharepointFolderName);

        var result = await _mediator.SendRequest(command, ct);
        await this.SendResponse(result, r => r.Value.ToDto());
    }
}
