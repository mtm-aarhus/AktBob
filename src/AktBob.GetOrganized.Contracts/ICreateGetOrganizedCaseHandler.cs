using AktBob.GetOrganized.Contracts.DTOs;

namespace AktBob.GetOrganized.Contracts;
public interface ICreateGetOrganizedCaseHandler
{
    Task<Result<CreateCaseResponse>> Handle(string caseTypePrefix, string caseTitle, string description, string status, string access, CancellationToken cancellationToken);
}