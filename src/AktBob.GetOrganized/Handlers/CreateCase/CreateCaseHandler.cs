using AAK.GetOrganized;
using AktBob.GetOrganized.Contracts.DTOs;
using ErrorOr;

namespace AktBob.GetOrganized.Handlers.CreateCase;
internal class CreateCaseHandler(IGetOrganizedClient getOrganizedClient) : ICreateCaseHandler
{
    private readonly IGetOrganizedClient _getOrganizedClient = getOrganizedClient;

    public async Task<ErrorOr<CreateCaseResponse>> Handle(
        string caseTitle,
        string caseProfile,
        string status,
        string access,
        string department,
        string facet,
        string kle,
        CancellationToken cancellationToken)
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
        
        var createCaseResponse = await _getOrganizedClient.CreateCase(createCaseCommand, cancellationToken);
        if (createCaseResponse == null)
        {
            return Error.Failure("GetOrganized.CreateCaseFailure", "Error creating GetOrganized case");
        }

        return new CreateCaseResponse(createCaseResponse.CaseId, createCaseResponse.CaseUrl);
    }
}
