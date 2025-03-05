using AAK.GetOrganized;
using AktBob.GetOrganized.Contracts;
using AktBob.GetOrganized.Contracts.DTOs;
using Ardalis.Result;

namespace AktBob.GetOrganized.Handlers;
internal class CreateCaseHandler(IGetOrganizedClient getOrganizedClient) : ICreateCaseHandler
{
    private readonly IGetOrganizedClient _getOrganizedClient = getOrganizedClient;

    public async Task<Result<CreateCaseResponse>> Handle(string caseTitle, string caseProfile, string status, string access, string department, string facet, string kle, CancellationToken cancellationToken)
    {
        var createCaseCommand = new AAK.GetOrganized.CreateCase.CreateAKTCaseCommand
        {
            Access = access,
            CaseProfile = caseProfile,
            CaseTitle = caseTitle,
            Department = department,
            Facet = facet,
            KLE = kle,
            Status = status
        };
        
        var createCaseResponse = await _getOrganizedClient.CreateCase(createCaseCommand);

        if (createCaseResponse == null)
        {
            return Result.Error();
        }

        var response = new CreateCaseResponse(createCaseResponse.CaseId, createCaseResponse.CaseUrl);
        return response;
    }
}
