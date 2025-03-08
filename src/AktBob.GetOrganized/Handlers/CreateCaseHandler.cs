using AAK.GetOrganized;
using AktBob.GetOrganized.Contracts;
using AktBob.GetOrganized.Contracts.DTOs;
using Ardalis.Result;

namespace AktBob.GetOrganized.Handlers;
internal class CreateCaseHandler(IGetOrganizedClient getOrganizedClient) : ICreateCaseHandler
{
    private readonly IGetOrganizedClient _getOrganizedClient = getOrganizedClient;

    public async Task<Result<CreateCaseResponse>> Handle(CreateGetOrganizedCaseCommand command, CancellationToken cancellationToken)
    {
        var createCaseCommand = new AAK.GetOrganized.CreateCase.CreateAKTCaseCommand
        {
            Access = command.Access,
            CaseProfile = command.CaseProfile,
            CaseTitle = command.CaseTitle,
            Department = command.Department,
            Facet = command.Facet,
            KLE = command.Kle,
            Status = command.Status
        };
        
        var createCaseResponse = await _getOrganizedClient.CreateCase(createCaseCommand);

        if (createCaseResponse == null)
        {
            return Result.Error("Error creating GetOrganized case");
        }

        var response = new CreateCaseResponse(createCaseResponse.CaseId, createCaseResponse.CaseUrl);
        return response;
    }
}
