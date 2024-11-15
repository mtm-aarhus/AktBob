using MediatR;
using Ardalis.Result;
using MigraDoc.DocumentObjectModel;
using Unit = MigraDoc.DocumentObjectModel.Unit;
using Document = MigraDoc.DocumentObjectModel.Document;
using MigraDoc.Rendering;
using AktBob.DocumentGenerator.Contracts;
using AktBob.DocumentGenerator;
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

            //// Add message number
            //section.AddMessageNumber(
            //    messageNumber: message.MessageNumber.ToString(),
            //    messageId: message.MessageId.ToString(),
            //    fontName: Constants.FONT_NAME_OPEN_SANS_SEMIBOLD,
            //    fontSize: Unit.FromPoint(12),
            //    fontColor: Color.Parse("0xFF000000"),
            //    spaceAfter: Unit.FromPoint(12));

            //// Add message timestamp
            //section.AddMessageTimestamp(
            //    text: message.CreatedAt.ToString("dd-MM-yyyy HH:mm:ss"),
            //    fontName: Constants.FONT_NAME_OPEN_SANS,
            //    fontSize: Unit.FromPoint(8),
            //    fontColor: Color.Parse("0xffaaaaaa"));

            //// Add "person from" name and email
            //section.AddPerson(
            //    name: message.PersonName,
            //    email: message.PersonEmail,
            //    fontName: Constants.FONT_NAME_OPEN_SANS,
            //    fontNameEmphasis: Constants.FONT_NAME_OPEN_SANS_SEMIBOLD,
            //    fontSize: Unit.FromPoint(8),
            //    fontColor: Color.Parse("0xffaaaaaa"),
            //    spaceAfter: Unit.FromPoint(2));

            //section.AddHorizontalLine(
            //    width: Unit.FromPoint(0.4f),
            //    color: Color.Parse("0xFFBBBBBB"),
            //    spaceBefore: Unit.FromPoint(10),
            //    spaceAfter: Unit.FromPoint(10));

            //section.ProcessMessageText(
            //    html: message.MessageContent,
            //    fontName: Constants.FONT_NAME_OPEN_SANS,
            //    fontSize: Unit.FromPoint(9),
            //    fontColor: Color.Parse("0xff000000"),
            //    linkColor: Color.Parse("0xFF4444FF"),
            //    spaceAfter: Unit.FromPoint(12));

            //// Attachments
            //section.AddHeadline("Bilag", Constants.FONT_NAME_OPEN_SANS_SEMIBOLD, Unit.FromPoint(7), Color.Parse("0xff000000"), Unit.FromPoint(0));

            //section.AddAttachmentList(
            //    filenames: message.AttachmentFileNames,
            //    fontName: Constants.FONT_NAME_OPEN_SANS,
            //    fontSize: Unit.FromPoint(7),
            //    fontColor: Color.Parse("0xff555555"),
            //    spaceAfter: Unit.FromPoint(2));

            //var spacer = section.AddParagraph();
            //spacer.Format.Font.Name = Constants.FONT_NAME_OPEN_SANS;
            //spacer.Format.SpaceAfter = Unit.FromPoint(24);
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
