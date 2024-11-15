using MigraDoc.DocumentObjectModel;
using System.Text.RegularExpressions;

namespace AktBob.DocumentGenerator;

internal static class Helpers
{

    public static void Configure(this Section section)
    {
        section.PageSetup = new PageSetup
        {
            TopMargin = Unit.FromPoint(30),
            LeftMargin = Unit.FromPoint(30),
            RightMargin = Unit.FromPoint(30),
            BottomMargin = Unit.FromPoint(30),
            PageFormat = PageFormat.A4,
            PageWidth = Unit.FromCentimeter(21), // Explicitly set A4 width
            PageHeight = Unit.FromCentimeter(29.7) // Explicitly set A4 height
        };
    }

    public static void AddPageNumberToFooter(this Section section, string fontName, Unit fontSize, Color fontColor, ParagraphAlignment alignment)
    {
        var footer = section.Footers.Primary.AddParagraph();
        footer.Format.Font.Name = fontName;
        footer.Format.Font.Size = fontSize;
        footer.Format.Font.Color = fontColor;
        footer.Format.Alignment = alignment;
        footer.AddText("Side ");
        footer.AddPageField();
        footer.AddText(" af ");
        footer.AddNumPagesField();
    }


    public static void AddHorizontalLine(this Section section, Unit width, Color color, Unit spaceBefore, Unit spaceAfter)
    {
        var paragraph = section.AddParagraph();
        paragraph.Format.Font = new Font(Constants.FONT_NAME_OPEN_SANS, Unit.FromPoint(0));
        paragraph.Format.SpaceBefore = spaceBefore;
        paragraph.Format.SpaceAfter = spaceAfter;

        paragraph.Format.Borders.Bottom = new Border
        {
            Width = width,
            Color = color
        };
    }

    public static void AddAttachmentList(this Section section, IEnumerable<string> filenames, string fontName, Unit fontSize, Color fontColor, Unit spaceAfter)
    {
        if (!filenames.Any())
        {
            return;
        }

        foreach (string filename in filenames)
        {
            if (string.IsNullOrWhiteSpace(filename)) continue;

            Paragraph paragraph = section.AddParagraph();
            paragraph.Format.Font.Name = fontName;
            paragraph.Format.Font.Size = fontSize;
            paragraph.Format.Font.Color = fontColor;
            paragraph.Format.SpaceAfter = spaceAfter;
            paragraph.AddText(filename);
        }
    }


    public static Section ProcessMessageText(this Section section, string html, string fontName, Unit fontSize, Color fontColor, Color linkColor, Unit spaceAfter)
    {
        // Split the HTML text into paragraphs based on the <p> tags
        string[] paragraphs = Regex.Split(html, @"<\/?p>");

        foreach (string paragraphHtml in paragraphs)
        {
            if (string.IsNullOrWhiteSpace(paragraphHtml)) continue;

            Paragraph paragraph = section.AddParagraph();
            paragraph.Format.Font.Name = fontName;
            paragraph.Format.Font.Size = fontSize;
            paragraph.Format.Font.Color = fontColor;
            paragraph.Format.SpaceAfter = spaceAfter;

            ProcessHtmlPart(paragraph, paragraphHtml, linkColor);
        }

        return section;
    }

    static void ProcessHtmlPart(Paragraph paragraph, string html, Color linkColor)
    {
        // Regex to match <span> with a style or <a> tags
        string spanPattern = @"<span\s+style=[""'](.*?)[""']>(.*?)<\/span>";
        string linkPattern = @"<a\s+href=[""'](.+?)[""']>(.*?)<\/a>";

        int lastIndex = 0;

        // Handle <span> and <a> tags within the same part
        foreach (Match match in Regex.Matches(html, $"{spanPattern}|{linkPattern}", RegexOptions.IgnoreCase))
        {
            // Add the text before the <span> or <a> as plain text
            if (match.Index > lastIndex)
            {
                string plainText = html.Substring(lastIndex, match.Index - lastIndex);
                AddTextWithLineBreaks(paragraph, plainText);
            }

            if (match.Groups[0].Value.StartsWith("<span", StringComparison.OrdinalIgnoreCase))
            {
                // Handle <span> tags
                string style = match.Groups[1].Value;
                string content = match.Groups[2].Value;

                // Check if the content contains an <a> tag or <br> tag
                if (Regex.IsMatch(content, linkPattern, RegexOptions.IgnoreCase))
                {
                    ProcessNestedLinks(paragraph, style, content, linkColor);
                }
                else
                {
                    var formattedText = paragraph.AddFormattedText(content);
                    ApplyStyle(formattedText, style);
                }
            }
            else if (match.Groups[0].Value.StartsWith("<a", StringComparison.OrdinalIgnoreCase))
            {
                // Handle <a> tags
                string href = match.Groups[3].Value;
                string linkText = match.Groups[4].Value;

                var hyperlink = paragraph.AddHyperlink(href, HyperlinkType.Web);
                var formattedLinkText = hyperlink.AddFormattedText(linkText);
                formattedLinkText.Color = linkColor;
                formattedLinkText.Underline = Underline.Single; // Optionally underline
            }

            // Update the last index
            lastIndex = match.Index + match.Length;
        }

        // Add any remaining text after the last match
        if (lastIndex < html.Length)
        {
            string remainingText = html.Substring(lastIndex);
            AddTextWithLineBreaks(paragraph, remainingText);
        }
    }

    static void ProcessNestedLinks(Paragraph paragraph, string style, string content, Color linkColor)
    {
        // Regex to match <a> and <br> tags
        string linkPattern = @"<a\s+href=[""'](.+?)[""']>(.*?)<\/a>";
        string lineBreakPattern = @"<br\s*/?>";

        int lastIndex = 0;
        foreach (Match match in Regex.Matches(content, $"{linkPattern}|{lineBreakPattern}", RegexOptions.IgnoreCase))
        {
            // Add the text before the <a> or <br> as formatted text
            if (match.Index > lastIndex)
            {
                string plainText = content.Substring(lastIndex, match.Index - lastIndex);
                var formattedText = paragraph.AddFormattedText(plainText);
                ApplyStyle(formattedText, style);
            }

            if (match.Groups[0].Value.StartsWith("<a", StringComparison.OrdinalIgnoreCase))
            {
                // Handle the <a> tag inside the <span>
                string href = match.Groups[1].Value;
                string linkText = match.Groups[2].Value;

                var hyperlink = paragraph.AddHyperlink(href, HyperlinkType.Web);
                var formattedLinkText = hyperlink.AddFormattedText(linkText);
                formattedLinkText.Color = linkColor; // Set hyperlink color
                formattedLinkText.Underline = Underline.Single; // Optionally underline

                // Apply the <span> style to the hyperlink text
                ApplyStyle(formattedLinkText, style);
            }
            else if (match.Groups[0].Value.StartsWith("<br", StringComparison.OrdinalIgnoreCase))
            {
                // Handle <br> tags by adding a line break
                paragraph.AddLineBreak();
            }

            // Update the last index
            lastIndex = match.Index + match.Length;
        }

        // Add any remaining text after the last <a> or <br> tag
        if (lastIndex < content.Length)
        {
            string remainingText = content.Substring(lastIndex);
            var formattedText = paragraph.AddFormattedText(remainingText);
            ApplyStyle(formattedText, style);
        }
    }

    static void ApplyStyle(FormattedText formattedText, string style)
    {
        // Apply styles based on the style string
        if (style.Contains("font-style:italic;"))
        {
            formattedText.Italic = true;
        }
        if (style.Contains("font-weight:bold;"))
        {
            formattedText.Bold = true;
        }
        // Additional styles can be handled here
    }

    static void AddTextWithLineBreaks(Paragraph paragraph, string text)
    {
        // Split the text by <br> tags
        string[] parts = Regex.Split(text, @"<br\s*/?>", RegexOptions.IgnoreCase);
        for (int i = 0; i < parts.Length; i++)
        {
            paragraph.AddText(parts[i]);
            if (i < parts.Length - 1)
            {
                paragraph.AddLineBreak();
            }
        }
    }


    public static void AddMessageNumber(this Section section, string messageNumber, string messageId, string fontName, Unit fontSize, Color fontColor, Unit spaceAfter)
    {
        var paragraph = section.AddParagraph();
        paragraph.Format.Font.Name = fontName;
        paragraph.Format.Font.Size = fontSize;
        paragraph.Format.Font.Color = fontColor;
        paragraph.AddText($"Besked nr: {messageNumber} (ID {messageId})");
        paragraph.Format.SpaceAfter = spaceAfter;
    }

    public static void AddMessageTimestamp(this Section section, string text, string fontName, Unit fontSize, Color fontColor)
    {
        var paragraph = section.AddParagraph();
        paragraph.Format.Font.Name = fontName;
        paragraph.Format.Font.Size = fontSize;
        paragraph.Format.Font.Color = fontColor;
        paragraph.AddText("Tidspunkt:");
        paragraph.AddTab();
        paragraph.AddText(text);
    }

    public static void AddPerson(this Section section, string name, string email, string fontName, string fontNameEmphasis, Unit fontSize, Color fontColor, Unit spaceAfter)
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

    public static void AddHeadline(this Section section, string text, string fontName, Unit fontSize, Color fontColor, Unit spaceAfter)
    {
        var paragraph = section.AddParagraph();
        paragraph.Format.Font.Name = fontName;
        paragraph.Format.Font.Size = fontSize;
        paragraph.Format.Font.Color = fontColor;
        paragraph.Format.SpaceAfter = spaceAfter;
        paragraph.AddText(text);
    }

}
