using AktBob.Database.Contracts;
using AktBob.Database.Dtos;
using AktBob.Shared.Types.Deskpro;
using FastEndpoints;
using Microsoft.AspNetCore.Http;

namespace AktBob.Database.Endpoints.OS2FormsSubmissions;

internal record GetCaseRequest(TicketId DeskproId);

internal class GetCase : Endpoint<GetCaseRequest, IEnumerable<RequestCaseDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetCase(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public override void Configure()
    {
        Get("/Cases");
        Options(x => x.WithTags("Cases"));
        Description(x => x.Produces<IEnumerable<RequestCaseDto>>(StatusCodes.Status200OK));
    }

    public async override Task HandleAsync(GetCaseRequest req, CancellationToken ct)
    {
        var tickets = await _unitOfWork.Tickets.GetAll(req.DeskproId, null, null);
        var dtos = new List<RequestCaseDto>();

        foreach (var ticket in tickets)
        {
            var submission = await _unitOfWork.OS2FormsSubmissions.GetByDeskproTicketId(req.DeskproId);

            var dto = new RequestCaseDto(
                DeskproId: req.DeskproId,
                CaseNumber: ticket.CaseNumber,
                CaseUrl: ticket.CaseUrl,
                FolderName: ticket.SharepointFolderName,
                RequestDescription: submission?.DescriptionFieldValue);

            dtos.Add(dto);
        }

        await SendOkAsync(dtos, ct);
    }
}
