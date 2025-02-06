namespace AktBob.CheckOCRScreeningStatus.UseCases;

public record RemoveCaseFromCacheCommand(Guid FilArkivCaseId);

public class RemoveCaseFromCacheCommandHandler(IData data) : MediatorRequestHandler<RemoveCaseFromCacheCommand>
{
    private readonly IData _data = data;

    protected override Task Handle(RemoveCaseFromCacheCommand request, CancellationToken cancellationToken)
    {
        var @case = _data.GetCase(request.FilArkivCaseId);
        if (@case != null)
        {
            _data.RemoveCase(@case);
        }

        return Task.CompletedTask;
    }
}
