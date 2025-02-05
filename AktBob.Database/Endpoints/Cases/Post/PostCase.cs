using AktBob.Database.Contracts.Dtos;
using AktBob.Database.Endpoints.Cases.Get;
using AktBob.Database.Extensions;
using AktBob.Database.UseCases.Cases.AddCase;
using FastEndpoints;
using MassTransit;
using MassTransit.Mediator;
using Microsoft.AspNetCore.Http;

namespace AktBob.Database.Endpoints.Cases.Post;
internal class PostCase(IMediator mediator) : Endpoint<PostCaseRequest, CaseDto>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Post("/Database/Cases");
        Options(x => x.WithTags("Database/Cases"));

        Description(x => x
            .Produces<CaseDto>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest));

        Summary(x =>
        {
            x.Description = "Opretter en ny case i databasen";
        });
    }

    public override async Task HandleAsync(PostCaseRequest req, CancellationToken ct)
    {
        var addCaseCommand = new AddCaseCommand(
            TicketId: req.TicketId,
            PodioItemId: req.PodioItemId,
            FilArkivCaseId: req.FilArkivCaseId,
            CaseNumber: req.CaseNumber);

        var result = await _mediator.SendRequest(addCaseCommand, ct);

        if (result.IsSuccess)
        {
            await SendCreatedAtAsync<GetCase>(routeValues: null, responseBody: result.Value, cancellation: ct);
            return;
        }

        await this.SendResponse(result, r => r.Value);
    }
}