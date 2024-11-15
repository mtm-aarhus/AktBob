using MediatR;
using MigraDoc.DocumentObjectModel;
using Unit = MigraDoc.DocumentObjectModel.Unit;

namespace AktBob.DocumentGenerator.UseCases.GenerateMessageContent;
internal class GenerateMessageContentCommandHandler : IRequestHandler<GenerateMessageContentCommand, Section>
{
    public Task<Section> Handle(GenerateMessageContentCommand command, CancellationToken cancellationToken)
    {
        var section = command.Section;
        var message = command.MessageDetailsDto;

        // Add message number
        section.AddMessageNumber(
            messageNumber: message.MessageNumber.ToString(),
            messageId: message.MessageId.ToString(),
            fontName: Constants.FONT_NAME_OPEN_SANS_SEMIBOLD,
            fontSize: Unit.FromPoint(12),
            fontColor: Color.Parse("0xFF000000"),
            spaceAfter: Unit.FromPoint(12));

        // Add message timestamp
        section.AddMessageTimestamp(
            text: message.CreatedAt.ToString("dd-MM-yyyy HH:mm:ss"),
            fontName: Constants.FONT_NAME_OPEN_SANS,
            fontSize: Unit.FromPoint(8),
            fontColor: Color.Parse("0xffaaaaaa"));

        // Add "person from" name and email
        section.AddPerson(
            name: message.PersonName,
            email: message.PersonEmail,
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
            html: message.MessageContent,
            fontName: Constants.FONT_NAME_OPEN_SANS,
            fontSize: Unit.FromPoint(9),
            fontColor: Color.Parse("0xff000000"),
            linkColor: Color.Parse("0xFF4444FF"),
            spaceAfter: Unit.FromPoint(12));

        // Attachments
        section.AddHeadline("Bilag", Constants.FONT_NAME_OPEN_SANS_SEMIBOLD, Unit.FromPoint(7), Color.Parse("0xff000000"), Unit.FromPoint(0));

        section.AddAttachmentList(
            filenames: message.AttachmentFileNames,
            fontName: Constants.FONT_NAME_OPEN_SANS,
            fontSize: Unit.FromPoint(7),
            fontColor: Color.Parse("0xff555555"),
            spaceAfter: Unit.FromPoint(2));

        var spacer = section.AddParagraph();
        spacer.Format.Font.Name = Constants.FONT_NAME_OPEN_SANS;
        spacer.Format.SpaceAfter = Unit.FromPoint(24);

        return Task.FromResult(section);
    }
}
