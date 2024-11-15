using Ardalis.Result;
using MediatR;

namespace AktBob.DocumentGenerator.Contracts;

public record GenerateDeskproMessageDocumentCommand(
    string TicketSubject,
    IEnumerable<MessageDetailsDto> MessageDetailsDtos) : IRequest<Result<byte[]>>;