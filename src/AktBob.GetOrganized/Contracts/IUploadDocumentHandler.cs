using Ardalis.Result;

namespace AktBob.GetOrganized.Contracts;
public interface IUploadDocumentHandler
{
    Task<Result<int>> Handle(UploadDocumentCommand command, CancellationToken cancellationToken);
}