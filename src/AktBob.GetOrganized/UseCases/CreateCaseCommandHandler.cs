using AAK.GetOrganized;
using AktBob.GetOrganized.Contracts;
using AktBob.Shared.CQRS;
using Ardalis.Result;

namespace AktBob.GetOrganized.UseCases;
internal class CreateCaseCommandHandler(IGetOrganizedClient getOrganizedClient) : ICommandHandler<CreateCaseCommand, Result<CreateCaseResponse>>
{
    private readonly IGetOrganizedClient _getOrganizedClient = getOrganizedClient;

    public async Task<Result<CreateCaseResponse>> Handle(CreateCaseCommand request, CancellationToken cancellationToken)
    {
        var createCaseCommand = new AAK.GetOrganized.CreateCase.CreateCaseCommand(request.CaseTypePrefix, request.CaseTitle, request.Description, request.Status, request.Access);
        var createCaseResponse = await _getOrganizedClient.CreateCase(createCaseCommand);

        if (createCaseResponse == null)
        {
            return Result.Error();
        }

        var response = new CreateCaseResponse(createCaseResponse.CaseId, createCaseResponse.CaseUrl);
        return response;
    }
}
