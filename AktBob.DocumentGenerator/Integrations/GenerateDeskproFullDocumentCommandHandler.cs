using AktBob.DocumentGenerator.Contracts;
using AktBob.DocumentGenerator.UseCases.GenerateMessageContent;
using Ardalis.Result;
using MediatR;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;
using Unit = MigraDoc.DocumentObjectModel.Unit;

namespace AktBob.DocumentGenerator.Integrations;
internal class GenerateDeskproFullDocumentCommandHandler(IMediator mediator) : IRequestHandler<GenerateDeskproFullDocumentCommand, Result<byte[]>>
{
    private readonly IMediator _mediator = mediator;

    public async Task<Result<byte[]>> Handle(GenerateDeskproFullDocumentCommand command, CancellationToken cancellationToken)
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
            text: command.DeskproTicketSubject,
            fontName: Constants.FONT_NAME_OPEN_SANS_SEMIBOLD,
            fontSize: Unit.FromPoint(14),
            fontColor: Color.Parse("0xFF000000"),
            spaceAfter: Unit.FromPoint(12));




        // Table rows
        var rowValues = command.TableRows.Select(t => new RowValue(t.Title, string.Join(", ", t.Values))).ToList();
        
        // Add table
        AddCaseDetailsTable(section, rowValues, Unit.FromPoint(24));

        section.AddHorizontalLine(
            width: Unit.FromPoint(0),
            color: Colors.Black,
            spaceBefore: Unit.FromPoint(0),
            spaceAfter: Unit.FromPoint(28));

        // Messages
        foreach (var message in command.MessageDetailsDtos)
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

    private void AddCaseDetailsTable(Section section, IEnumerable<RowValue> rowValues, Unit spaceAfter)
    {
        // Access page setup properties
        Unit pageWidth = section.PageSetup.PageWidth;
        Unit leftMargin = section.PageSetup.LeftMargin;
        Unit rightMargin = section.PageSetup.RightMargin;

        // Calculate the usable width
        Unit usableWidth = (pageWidth * 0.95) - leftMargin - rightMargin;

        // Add table
        var table = section.AddTable();
        table.Borders.Width = 0.75; // Border width
        table.Borders.Color = Colors.Black;

        Column column1 = table.AddColumn(usableWidth * 0.30);
        column1.Format.Alignment = ParagraphAlignment.Left;

        Column column2 = table.AddColumn(usableWidth * 0.7);
        column2.Format.Alignment = ParagraphAlignment.Left;

        // Add some rows
        for (int i = 0; i < rowValues.Count(); i++)
        {
            var isEvenRow = i%2 == 0;
            AddRow(table, isEvenRow, rowValues.ElementAt(i).Title, rowValues.ElementAt(i).Value);
        }

    }

    private void AddRow(Table table, bool striped, string title, string value)
    {
        Row row = table.AddRow();
        
        if (striped)
        {
            row.Shading.Color = Color.Parse("0xFFc1e9f7");
        }

        var titleCell = row.Cells[0].AddParagraph(title);
        SetCellPadding(titleCell);
        titleCell.Format.Font.Name = Constants.FONT_NAME_OPEN_SANS_SEMIBOLD;
        titleCell.Format.Font.Size = Unit.FromPoint(9);
        titleCell.Format.Font.Color = Colors.Black;
        

        var valueCell = row.Cells[1].AddParagraph(value);
        SetCellPadding(valueCell);
        valueCell.Format.Font.Name = Constants.FONT_NAME_OPEN_SANS;
        valueCell.Format.Font.Size = Unit.FromPoint(9);
        valueCell.Format.Font.Color = Colors.Black;
    }

    private void SetCellPadding(Paragraph paragraph)
    {
        paragraph.Format.SpaceBefore = Unit.FromPoint(2);
        paragraph.Format.SpaceAfter = Unit.FromPoint(2);
        paragraph.Format.LeftIndent = Unit.FromPoint(2);
        paragraph.Format.RightIndent = Unit.FromPoint(2);
    }

    private record RowValue(string Title, string Value);
}
