using AAK.FilArkiv;
using AktBob.FilArkiv.Contracts.DTOs;
using ErrorOr;

namespace AktBob.FilArkiv.Handlers.GetFileProcessStatus;
internal class GetFileProcessStatusHandler : IGetFileProcessStatusHandler
{
    private readonly IFilArkiv _filArkiv;

    public GetFileProcessStatusHandler(IFilArkiv filArkiv)
    {
        _filArkiv = filArkiv;
    }
    public async Task<ErrorOr<FileProcessStatusDto>> Handle(Guid fileId, CancellationToken cancellationToken = default)
    {
        var fileProcessStatus = await _filArkiv.GetFileProcessStatus(fileId, cancellationToken);
        return new FileProcessStatusDto(fileProcessStatus.IsInQueue, fileProcessStatus.IsBeingProcessed);
    }
}
