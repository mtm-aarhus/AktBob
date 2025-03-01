using AAK.GetOrganized;
using AktBob.GetOrganized.Contracts;
using AktBob.GetOrganized.Contracts.DTOs;
using Ardalis.Result;

namespace AktBob.GetOrganized.Handlers;
internal class CreateGetOrganizedCaseHandler(IGetOrganizedClient getOrganizedClient) : ICreateGetOrganizedCaseHandler
{
    private readonly IGetOrganizedClient _getOrganizedClient = getOrganizedClient;

    public async Task<Result<CreateCaseResponse>> Handle(string caseTypePrefix, string caseTitle, string description, string status, string access, CancellationToken cancellationToken)
    {
        var createCaseCommand = new AAK.GetOrganized.CreateCase.CreateCaseCommand(caseTypePrefix, caseTitle, description, status, access);
        var createCaseResponse = await _getOrganizedClient.CreateCase(createCaseCommand);

        if (createCaseResponse == null)
        {
            return Result.Error();
        }

        var response = new CreateCaseResponse(createCaseResponse.CaseId, createCaseResponse.CaseUrl);
        return response;
    }
}
