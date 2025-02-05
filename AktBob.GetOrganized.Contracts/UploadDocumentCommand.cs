using AAK.GetOrganized.UploadDocument;
using Ardalis.Result;
using MassTransit.Mediator;

namespace AktBob.GetOrganized.Contracts;
public record UploadDocumentCommand(byte[] Bytes, string CaseNumber, string FileName, UploadDocumentMetadata Metadata, bool Overwrite) : Request<Result<int>>;