using AktBob.Database.Contracts;
using AktBob.Database.Dtos;
using AktBob.Database.Entities;
using AktBob.Database.Extensions;
using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace AktBob.Database.Endpoints.Cases;

internal record PostCaseRequest
{
    public int TicketId { get; set; }
    public long PodioItemId { get; set; }
    public string CaseNumber { get; set; } = string.Empty;
    public Guid? FilArkivCaseId { get; set; }
}

internal class PostCaseRequestValidator : Validator<PostCaseRequest>
{
    public PostCaseRequestValidator()
    {
        RuleFor(x => x.TicketId).NotEmpty();
        RuleFor(x => x.PodioItemId).NotEmpty();
        RuleFor(x => x.CaseNumber).NotEmpty();
    }
}


internal class PostCase(ICaseRepository caseRepository) : Endpoint<PostCaseRequest, CaseDto>
{
    private readonly ICaseRepository _caseRepository = caseRepository;

    public override void Configure()
    {
        Post("/Database/Cases");
        Options(x => x.WithTags("Database/Cases"));

        Description(x => x
            .Produces<CaseDto>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest));
    }

    public override async Task HandleAsync(PostCaseRequest req, CancellationToken ct)
    {
        var @case = new Case
        {
            TicketId = req.TicketId,
            PodioItemId = req.PodioItemId,
            CaseNumber = req.CaseNumber,
            FilArkivCaseId = req.FilArkivCaseId
        };

        var success = await _caseRepository.Add(@case);

        if (!success)
        {
            await SendErrorsAsync(500, ct);
            return;
        }

        await SendOkAsync(@case.ToDto(), ct);
    }
}