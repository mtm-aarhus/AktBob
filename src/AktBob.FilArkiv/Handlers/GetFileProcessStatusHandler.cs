using AAK.FilArkiv;
using AktBob.FilArkiv.Contracts;
using Ardalis.Result;

namespace AktBob.FilArkiv.Handlers;
internal class GetFileProcessStatusHandler : IGetFileProcessStatusHandler
{
    private readonly IFilArkiv _filArkiv;

    public GetFileProcessStatusHandler(IFilArkiv filArkiv)
    {
        _filArkiv = filArkiv;
    }
    public async Task<Result<FileProcessStatusDto>> Handle(Guid fileId, CancellationToken cancellationToken = default)
    {
        var fileProcessStatus = await _filArkiv.GetFileProcessStatus(fileId, cancellationToken);
        return new FileProcessStatusDto(fileProcessStatus.IsInQueue, fileProcessStatus.IsBeingProcessed);
    }
}
