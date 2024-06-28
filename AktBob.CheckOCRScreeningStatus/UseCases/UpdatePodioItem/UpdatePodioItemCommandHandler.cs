using Ardalis.Result;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AktBob.CheckOCRScreeningStatus.UseCases.UpdatePodioItem;
internal class UpdatePodioItemCommandHandler : IRequestHandler<UpdatePodioItemCommand, Result>
{
    private readonly IData _data;
    private readonly IConfiguration _configuration;
    private readonly IMediator _mediator;
    private readonly ILogger<UpdatePodioItemCommandHandler> _logger;
    private readonly IAktBobApi _aktBobApi;

    public UpdatePodioItemCommandHandler(IData data, IConfiguration configuration, IMediator mediator, ILogger<UpdatePodioItemCommandHandler> logger, IAktBobApi aktBobApi)
    {
        _data = data;
        _configuration = configuration;
        _mediator = mediator;
        _logger = logger;
        _aktBobApi = aktBobApi;
    }

    public async Task<Result> Handle(UpdatePodioItemCommand request, CancellationToken cancellationToken)
    {
        var @case = _data.GetCase(request.CaseId);

        if (@case == null)
        {
            return Result.Error(new ErrorList([$"Case {request.CaseId} not found"], string.Empty));
        }

        if (@case.PodioItemUpdated)
        {
            return Result.SuccessWithMessage($"Case {request.CaseId}: PodioItem already updated");
        }

        return await _aktBobApi.UpdatePodioItemFilArkivField(@case.PodioItemId, @case.CaseId);
    }
}
