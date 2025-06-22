using AAK.FilArkiv;
using AktBob.Shared.Contracts.Modules.FilArkiv.Contracts.DTOs;
using ErrorOr;

namespace Aktbob.Modules.FilArkiv.Features.GetFileProcessStatus;
internal class GetFileProcessStatusHandler(IFilArkiv filArkiv) : IGetFileProcessStatusHandler
{
    public async Task<ErrorOr<FileProcessStatusDto>> Handle(Guid fileId, CancellationToken cancellationToken = default)
    {
        var fileProcessStatus = await filArkiv.GetFileProcessStatus(fileId, cancellationToken);
        return new FileProcessStatusDto(fileProcessStatus.IsInQueue, fileProcessStatus.IsBeingProcessed);
    }
}
