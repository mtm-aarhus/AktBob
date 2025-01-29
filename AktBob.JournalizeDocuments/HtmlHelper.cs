using AktBob.Deskpro.Contracts.DTOs;

namespace AktBob.JournalizeDocuments;
internal static class HtmlHelper
{
    public static string GenerateHtml(string templateFileName, Dictionary<string, string> dictionary)
    {
        var template = File.ReadAllText(templateFileName); // TODO: cache
        var html = template.ReplacePlaceholders(dictionary);
        return html;
    }

    public static IEnumerable<string> GenerateListOfFieldValues(int[] fieldIds, TicketDto ticketDto, string templateFileName)
    {
        List<string> items = new();

        foreach (var fieldId in fieldIds)
        {
            var values = ticketDto.Fields.FirstOrDefault(f => f.Id == fieldId)?.Values ?? Enumerable.Empty<string>();

            if (values.Count() == 0)
            {
                continue;
            }

            var value = string.Join(", ", values);
            var kvp = new KeyValuePair<string, string>("value", value);
            var html = GenerateHtml(templateFileName, kvp.ToDictionary());
            items.Add(html);
        }

        return items;
    }

    public static string GenerateMessageHtml(MessageDto deskproMessageDto, IEnumerable<AttachmentDto> attachments, string goCaseNumber, string caseTitle, int messageNumber)
    {
        var htmlTemplate = File.ReadAllText("HTMLTemplates/message.html") ?? string.Empty;
        var attachmentFileNames = attachments.Select(a =>
            GenerateHtml(
                "HTMLTemplates/message-attachments.html",
                new KeyValuePair<string, string>("value", a.FileName).ToDictionary()));

        var dictionary = new Dictionary<string, string>
        {
            { "caseNumber",  goCaseNumber },
            { "title", caseTitle },
            { "messageNumber", messageNumber.ToString() ?? string.Empty },
            { "timestamp", deskproMessageDto.CreatedAt.ToString("dd-MM-yyyy HH:mm:ss") },
            { "fromName", deskproMessageDto.Person.FullName },
            { "fromEmail", deskproMessageDto.Person.Email },
            { "attachments", string.Join("", attachmentFileNames) },
            { "messageContent", deskproMessageDto.Content }
        };

        var html = htmlTemplate.ReplacePlaceholders(dictionary);
        return html;
    }
}
