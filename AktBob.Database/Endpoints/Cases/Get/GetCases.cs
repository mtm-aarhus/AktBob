using AktBob.Database.Contracts.Dtos;
using AktBob.Database.Extensions;
using AktBob.Database.UseCases.Cases.GetCases;
using FastEndpoints;
using MassTransit;
using MassTransit.Mediator;
using Microsoft.AspNetCore.Http;

namespace AktBob.Database.Endpoints.Cases.Get;
internal class GetCases(IMediator mediator) : Endpoint<GetCasesRequest, IEnumerable<CaseDto>>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Get("/Database/Cases");
        Options(x => x.WithTags("Database/Cases"));

        Description(x => x
            .Produces<IEnumerable<CaseDto>>(StatusCodes.Status200OK));

        Summary(x =>
        {
            x.Description = "Returnerer et array med cases, der matcher den eller de angivne filteringsfelter. Alle felter er valgfrie. Hvis ingen angives, returneres samtlige cases fra databasen.";
        });
    }

    public override async Task HandleAsync(GetCasesRequest req, CancellationToken ct)
    {
        var query = new GetCasesQuery(
            DeskproId: req.DeskproId,
            PodioItemId: req.PodioItemId,
            FilArkivCaseId: req.FilArkivCaseId);

        var result = await _mediator.SendRequest(query, ct);
        await this.SendResponse(result, r => r.Value);
    }
}
