using AktBob.Database.Entities;
using AktBob.Database.Contracts.Dtos;

namespace AktBob.Database.Extensions;
internal static class CaseExtensions
{
    public static CaseDto ToDto(this Case @case)
    {
        return new CaseDto
        {
            Id = @case.Id,
            TicketId = @case.TicketId,
            FilArkivCaseId = @case.FilArkivCaseId,
            CaseNumber = @case.CaseNumber,
            PodioItemId = @case.PodioItemId,
            SharepointFolderName = @case.SharepointFolderName
        };
    }

    public static IEnumerable<CaseDto> ToDto(this IEnumerable<Case> cases) => cases.Select(c => c.ToDto());
}
