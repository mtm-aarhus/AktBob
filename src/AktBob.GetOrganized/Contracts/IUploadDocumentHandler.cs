using AAK.GetOrganized.UploadDocument;
using Ardalis.Result;

namespace AktBob.GetOrganized.Contracts;
public interface IUploadDocumentHandler
{
    Task<Result<int>> Handle(byte[] bytes, string caseNumber, string fileName, UploadDocumentMetadata metadata, bool overwrite, CancellationToken cancellationToken);
}