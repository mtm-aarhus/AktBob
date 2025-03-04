using AktBob.GetOrganized.Contracts.DTOs;
using Ardalis.Result;

namespace AktBob.GetOrganized.Contracts;
internal interface ICreateCaseHandler
{
    Task<Result<CreateCaseResponse>> Handle(string caseTypePrefix, string caseTitle, string description, string status, string access, CancellationToken cancellationToken);
}