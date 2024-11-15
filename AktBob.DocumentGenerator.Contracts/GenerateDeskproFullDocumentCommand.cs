using Ardalis.Result;
using MediatR;

namespace AktBob.DocumentGenerator.Contracts;
public record GenerateDeskproFullDocumentCommand(
    int DeskproTicketId,
    string DeskproTicketSubject,
    IEnumerable<TableRowDto> TableRows,
    IEnumerable<MessageDetailsDto> MessageDetailsDtos
    ) : IRequest<Result<byte[]>>;