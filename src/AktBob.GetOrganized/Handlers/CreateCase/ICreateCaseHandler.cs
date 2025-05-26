using AktBob.GetOrganized.Contracts.DTOs;
using ErrorOr;

namespace AktBob.GetOrganized.Handlers.CreateCase;
internal interface ICreateCaseHandler
{
    Task<ErrorOr<CreateCaseResponse>> Handle(
        string caseTitle,
        string caseProfile,
        string status,
        string access,
        string department,
        string facet,
        string kle,
        CancellationToken cancellationToken);
}