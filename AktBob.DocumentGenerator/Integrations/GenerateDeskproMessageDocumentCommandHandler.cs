using MediatR;
using Ardalis.Result;
using MigraDoc.DocumentObjectModel;
using Unit = MigraDoc.DocumentObjectModel.Unit;
using Document = MigraDoc.DocumentObjectModel.Document;
using MigraDoc.Rendering;
using AktBob.DocumentGenerator.Contracts;
using AktBob.DocumentGenerator.UseCases.GenerateMessageContent;

namespace AktBob.DocumentGenerator.Integrations;
internal class GenerateDeskproMessageDocumentCommandHandler(IMediator mediator) : IRequestHandler<GenerateDeskproMessageDocumentCommand, Result<byte[]>>
{
    private readonly IMediator _mediator = mediator;

    public async Task<Result<byte[]>> Handle(GenerateDeskproMessageDocumentCommand request, CancellationToken cancellationToken)
    {
        // Create document object
        var document = new Document();
        document.DefaultTabStop = Unit.FromPoint(50);

        // Add section
        var section = document.AddSection();
        section.Configure();

        // Add page numbering
        section.AddPageNumberToFooter(
            fontName: Constants.FONT_NAME_OPEN_SANS,
            fontSize: Unit.FromPoint(7),
            fontColor: Color.Parse("0xFF999999"),
            alignment: ParagraphAlignment.Right);

        // Add headline
        section.AddHeadline(
            text: request.TicketSubject,
            fontName: Constants.FONT_NAME_OPEN_SANS_SEMIBOLD,
            fontSize: Unit.FromPoint(12),
            fontColor: Color.Parse("0xFFAAAAAA"),
            spaceAfter: Unit.FromPoint(12));

        foreach (var message in request.MessageDetailsDtos)
        {
            var generateMessageContentCommand = new GenerateMessageContentCommand(section, message);
            await _mediator.Send(generateMessageContentCommand, cancellationToken);
        }

        // Render the document as a PDF
        var renderer = new PdfDocumentRenderer();
        renderer.Document = document;
        renderer.RenderDocument();

        using (var stream = new MemoryStream())
        {
            renderer.Save(stream, false);
            var bytes = stream.ToArray();
            return Result.Success(bytes);
        }
    }
}
