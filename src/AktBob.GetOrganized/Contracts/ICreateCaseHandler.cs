using AktBob.GetOrganized.Contracts.DTOs;
using Ardalis.Result;

namespace AktBob.GetOrganized.Contracts;
internal interface ICreateCaseHandler
{
    Task<Result<CreateCaseResponse>> Handle(string caseTitle, string caseProfile, string status, string access, string department, string facet, string kle, CancellationToken cancellationToken);
}