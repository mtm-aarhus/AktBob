using MediatR;
using Microsoft.Extensions.Logging;

namespace AktBob.CheckOCRScreeningStatus.UseCases.RemoveCaseFromCache;
internal class RemoveCaseFromCacheCommandHandler : IRequestHandler<RemoveCaseFromCacheCommand>
{
    private readonly IData _data;
    private readonly ILogger<RemoveCaseFromCacheCommandHandler> _logger;

    public RemoveCaseFromCacheCommandHandler(IData data, ILogger<RemoveCaseFromCacheCommandHandler> logger)
    {
        _data = data;
        _logger = logger;
    }

    public Task Handle(RemoveCaseFromCacheCommand request, CancellationToken cancellationToken)
    {
        var @case = _data.GetCase(request.CaseId);

        if (@case != null)
        {
            _data.RemoveCase(@case);
        }

        _logger.LogInformation("Case {id} has been removed from cache", request.CaseId);
        return Task.CompletedTask;
    }
}
