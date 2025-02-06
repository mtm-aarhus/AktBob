using AAK.GetOrganized;
using AktBob.GetOrganized.Contracts;
using Ardalis.Result;
using MassTransit.Mediator;

namespace AktBob.GetOrganized.UseCases;
public class CreateCaseCommandHandler(IGetOrganizedClient getOrganizedClient) : MediatorRequestHandler<CreateCaseCommand, Result<CreateCaseResponse>>
{
    private readonly IGetOrganizedClient _getOrganizedClient = getOrganizedClient;

    protected override async Task<Result<CreateCaseResponse>> Handle(CreateCaseCommand request, CancellationToken cancellationToken)
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
