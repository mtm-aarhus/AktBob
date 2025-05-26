using AktBob.FilArkiv.Contracts.DTOs;
using ErrorOr;

namespace AktBob.FilArkiv.Handlers.GetFileProcessStatus;
internal interface IGetFileProcessStatusHandler
{
    Task<ErrorOr<FileProcessStatusDto>> Handle(Guid fileId, CancellationToken cancellationToken = default);
}
