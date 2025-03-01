using AktBob.Database.Contracts;
using AktBob.Database.Dtos;
using AktBob.Database.Extensions;
using FastEndpoints;
using Microsoft.AspNetCore.Http;

namespace AktBob.Database.Endpoints.Cases;

internal record GetCaseRequest(int Id);

internal class GetCase(ICaseRepository caseRepository) : Endpoint<GetCaseRequest, CaseDto>
{
    private readonly ICaseRepository _caseRepository = caseRepository;

    public override void Configure()
    {
        Get("/Database/Cases/{Id}");
        Options(x => x.WithTags("Database/Cases"));

        Description(x => x
            .Produces<CaseDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound));
    }

    public override async Task HandleAsync(GetCaseRequest req, CancellationToken ct)
    {
        var @case = await _caseRepository.Get(req.Id);

        if (@case == null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        await SendOkAsync(@case.ToDto(), ct);
    }
}