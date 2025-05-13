using Ardalis.Result;

namespace AktBob.FilArkiv.Contracts;
internal interface IGetFileProcessStatusHandler
{
    Task<Result<FileProcessStatusDto>> Handle(Guid fileId, CancellationToken cancellationToken = default);
}
