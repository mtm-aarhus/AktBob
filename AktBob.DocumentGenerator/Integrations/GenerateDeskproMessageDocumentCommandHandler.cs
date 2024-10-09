using MediatR;
using Ardalis.Result;
using MigraDoc.DocumentObjectModel;
using Unit = MigraDoc.DocumentObjectModel.Unit;
using Document = MigraDoc.DocumentObjectModel.Document;
using MigraDoc.Rendering;
using AktBob.DocumentGenerator.Contracts;

namespace AktBob.DocumentGenerator.Integrations;
internal class GenerateDeskproMessageDocumentCommandHandler : IRequestHandler<GenerateDeskproMessageDocumentCommand, Result<byte[]>>
{
    public Task<Result<byte[]>> Handle(GenerateDeskproMessageDocumentCommand request, CancellationToken cancellationToken)
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
        AddHeadline(
            section: section,
            text: request.TicketSubject,
            fontName: Constants.FONT_NAME_OPEN_SANS_SEMIBOLD,
            fontSize: Unit.FromPoint(12),
            fontColor: Color.Parse("0xFFAAAAAA"));

        // Add message number
        AddMessageNumber(
            section: section,
            messageNumber: request.MessageNumber.ToString(),
            messageId: request.MessageId.ToString(),
            fontName: Constants.FONT_NAME_OPEN_SANS_SEMIBOLD,
            fontSize: Unit.FromPoint(12),
            fontColor: Color.Parse("0xFF000000"),
            spaceAfter: Unit.FromPoint(24));

        // Add message timestamp
        AddMessageTimestamp(
            section: section,
            text: request.CreatedAt.ToString("dd-MM-yyyy HH:mm:ss"),
            fontName: Constants.FONT_NAME_OPEN_SANS,
            fontSize: Unit.FromPoint(8),
            fontColor: Color.Parse("0xffaaaaaa"));

        // Add "person from" name and email
        AddPerson(
            section: section,
            name: request.PersonName,
            email: request.PersonEmail,
            fontName: Constants.FONT_NAME_OPEN_SANS,
            fontNameEmphasis: Constants.FONT_NAME_OPEN_SANS_SEMIBOLD,
            fontSize: Unit.FromPoint(8),
            fontColor: Color.Parse("0xffaaaaaa"),
            spaceAfter: Unit.FromPoint(2));

        section.AddHorizontalLine(
            width: Unit.FromPoint(0.4f),
            color: Color.Parse("0xFFBBBBBB"),
            spaceBefore: Unit.FromPoint(10),
            spaceAfter: Unit.FromPoint(10));

        section.ProcessMessageText(
            html: request.MessageContent,
            fontName: Constants.FONT_NAME_OPEN_SANS,
            fontSize: Unit.FromPoint(9),
            fontColor: Color.Parse("0xff000000"),
            linkColor: Color.Parse("0xFF4444FF"),
            spaceAfter: Unit.FromPoint(12));

        // Render the document as a PDF
        var renderer = new PdfDocumentRenderer();
        renderer.Document = document;
        renderer.RenderDocument();

        using (var stream = new MemoryStream())
        {
            renderer.Save(stream, false);
            var bytes = stream.ToArray();
            return Task.FromResult(Result.Success(bytes));
        }
    }


    private void AddMessageNumber(Section section, string messageNumber, string messageId, string fontName, Unit fontSize, Color fontColor, Unit spaceAfter)
    {
        var paragraph = section.AddParagraph();
        paragraph.Format.Font.Name = fontName;
        paragraph.Format.Font.Size = fontSize;
        paragraph.Format.Font.Color = fontColor;
        paragraph.AddText($"Besked nr: {messageNumber} (ID {messageId})");
        paragraph.Format.SpaceAfter = spaceAfter;
    }

    private void AddMessageTimestamp(Section section, string text, string fontName, Unit fontSize, Color fontColor)
    {
        var paragraph = section.AddParagraph();
        paragraph.Format.Font.Name = fontName;
        paragraph.Format.Font.Size = fontSize;
        paragraph.Format.Font.Color = fontColor;
        paragraph.AddText("Tidspunkt:");
        paragraph.AddTab();
        paragraph.AddText(text);
    }

    private void AddPerson(Section section, string name, string email, string fontName, string fontNameEmphasis, Unit fontSize, Color fontColor, Unit spaceAfter)
    {
        var paragraph = section.AddParagraph();
        paragraph.Format.Font.Name = fontName;
        paragraph.Format.Font.Size = fontSize;
        paragraph.Format.Font.Color = fontColor;
        paragraph.AddText("Fra:");
        paragraph.AddTab();
        paragraph.AddFormattedText(name, new Font(fontNameEmphasis));
        paragraph.AddSpace(2);
        paragraph.AddFormattedText($"< {email} >", new Font(fontName));
        paragraph.Format.SpaceAfter = spaceAfter;
    }

    public void AddHeadline(Section section, string text, string fontName, Unit fontSize, Color fontColor)
    {
        var paragraph = section.AddParagraph();
        paragraph.Format.Font.Name = fontName;
        paragraph.Format.Font.Size = fontSize;
        paragraph.Format.Font.Color = fontColor;
        paragraph.AddText(text);
    }
}
