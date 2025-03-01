using AAK.GetOrganized.UploadDocument;

namespace AktBob.GetOrganized.Contracts;
public interface IUploadGetOrganizedDocumentHandler
{
    Task<Result<int>> Handle(byte[] bytes, string caseNumber, string fileName, UploadDocumentMetadata metadata, bool overwrite, CancellationToken cancellationToken);
}