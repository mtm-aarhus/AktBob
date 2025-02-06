using AktBob.Database.Contracts.Dtos;
using AktBob.Database.Extensions;
using AktBob.Database.UseCases.Cases;
using FastEndpoints;
using MassTransit;
using MassTransit.Mediator;
using Microsoft.AspNetCore.Http;

namespace AktBob.Database.Endpoints.Cases;

internal record GetCaseRequest(int Id);

internal class GetCase(IMediator mediator) : Endpoint<GetCaseRequest, CaseDto>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Get("/Database/Cases/{Id}");
        Options(x => x.WithTags("Database/Cases"));

        Description(x => x
            .Produces<CaseDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound));

        Summary(x =>
        {
            x.Description = "Henter en specific case fra databasen ud fra database-ID'et";
        });
    }

    public override async Task HandleAsync(GetCaseRequest req, CancellationToken ct)
    {
        var getCaseByIdQuery = new GetCaseByIdQuery(req.Id);
        var result = await _mediator.SendRequest(getCaseByIdQuery, ct);

        await this.SendResponse(result, r => r.Value);
    }
}