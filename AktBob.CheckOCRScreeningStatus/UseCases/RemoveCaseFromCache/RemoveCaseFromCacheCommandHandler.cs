using Ardalis.Result;
using MediatR;

namespace AktBob.CheckOCRScreeningStatus.UseCases.RemoveCaseFromCache;
internal class RemoveCaseFromCacheCommandHandler : IRequestHandler<RemoveCaseFromCacheCommand, Result>
{
    private readonly IData _data;

    public RemoveCaseFromCacheCommandHandler(IData data)
    {
        _data = data;
    }

    public Task<Result> Handle(RemoveCaseFromCacheCommand request, CancellationToken cancellationToken)
    {
        var @case = _data.GetCase(request.CaseId);

        if (@case != null)
        {
            _data.RemoveCase(@case);
        }

        return Task.FromResult(Result.SuccessWithMessage($"Case {request.CaseId} has been removed from cache"));
    }
}
