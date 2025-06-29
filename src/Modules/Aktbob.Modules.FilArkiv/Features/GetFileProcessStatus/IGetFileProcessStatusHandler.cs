using AktBob.Shared.Contracts.Modules.FilArkiv.Contracts.DTOs;
using ErrorOr;

namespace Aktbob.Modules.FilArkiv.Features.GetFileProcessStatus;
public interface IGetFileProcessStatusHandler
{
    Task<ErrorOr<FileProcessStatusDto>> Handle(Guid fileId, CancellationToken cancellationToken = default);
}
