using AAK.GetOrganized.UploadDocument;

namespace AktBob.GetOrganized.Contracts;
public record UploadDocumentCommand(byte[] Bytes, string CaseNumber, string FileName, UploadDocumentMetadata Metadata, bool Overwrite) : ICommand<Result<int>>;