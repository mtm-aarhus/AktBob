using Ardalis.Result;
using MediatR;

namespace AktBob.DocumentGenerator.Contracts;

public record GenerateDeskproMessageDocumentCommand(
    string TicketSubject,
    int MessageId,
    int MessageNumber,
    string MessageContent,
    DateTime CreatedAt,
    string PersonName,
    string PersonEmail) : IRequest<Result<byte[]>>;