using AktBob.Database.Contracts.Dtos;
using AktBob.Database.Extensions;
using AktBob.Database.UseCases.Cases.GetCases;
using FastEndpoints;
using Microsoft.AspNetCore.Http;

namespace AktBob.Database.Endpoints.Cases;

internal record GetCasesRequest(int? DeskproId, long? PodioItemId, Guid? FilArkivCaseId);

internal class GetCases(IMediator mediator) : Endpoint<GetCasesRequest, IEnumerable<CaseDto>>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Get("/Database/Cases");
        Options(x => x.WithTags("Database/Cases"));

        Description(x => x
            .Produces<IEnumerable<CaseDto>>(StatusCodes.Status200OK));
    }

    public override async Task HandleAsync(GetCasesRequest req, CancellationToken ct)
    {
        var query = new GetCasesQuery(
            DeskproId: req.DeskproId,
            PodioItemId: req.PodioItemId,
            FilArkivCaseId: req.FilArkivCaseId);

        var result = await _mediator.Send(query, ct);
        await this.SendResponse(result, r => r.Value);
    }
}
