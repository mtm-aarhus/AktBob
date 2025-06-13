using AktBob.Database.Contracts;
using AktBob.Shared.Contracts.Database;
using Microsoft.AspNetCore.Mvc;

namespace AktBob.Api.Endpoints.Submissions;

internal static class SearchSubmissions
{
    private record GetCaseRequest(int DeskproId);

    public static void MapSearchSubmissionsEndpoint(this RouteGroupBuilder builder, string route) => builder
        .MapGet(route, Endpoint)
        .WithSummary("Fremsøg submissions")
        .WithDescription("Fremsøger de matchende submissions ud fra det angivne Deskpro ID")
        .Produces<SubmissionDto[]>();

    private static async Task<IResult> Endpoint(
        [AsParameters] GetCaseRequest request,
        [FromServices] IUnitOfWork unitOfWork)
    {
        var tickets = await unitOfWork.Tickets.GetAll(request.DeskproId, null, null);
        var dtos = new List<SubmissionDto>();

        foreach (var ticket in tickets)
        {
            var submission = await unitOfWork.OS2FormsSubmissions.GetByDeskproTicketId(request.DeskproId);

            var dto = new SubmissionDto(
                DeskproId: request.DeskproId,
                CaseNumber: ticket.CaseNumber,
                CaseUrl: ticket.CaseUrl,
                FolderName: ticket.SharepointFolderName,
                RequestDescription: submission?.DescriptionFieldValue);

            dtos.Add(dto);
        }

        return Results.Ok(dtos);
    }
}