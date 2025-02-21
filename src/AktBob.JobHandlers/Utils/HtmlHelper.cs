using AktBob.Deskpro.Contracts.DTOs;
using AktBob.JobHandlers.Extensions;

namespace AktBob.JobHandlers.Utils;
internal static class HtmlHelper
{
    public static string GenerateHtml(string templateFileName, Dictionary<string, string> dictionary)
    {
        string appRoot = AppDomain.CurrentDomain.BaseDirectory;
        string templatePath = Path.Combine(appRoot, "HtmlTemplates", templateFileName);
        var template = File.ReadAllText(templatePath); // TODO: cache
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

    public static string GenerateMessageHtml(DateTime createdAt, string personName, string personEmail, string content, string caseNumber, string caseTitle, int messageNumber, IEnumerable<AttachmentDto> attachments)
    {
        string appRoot = AppDomain.CurrentDomain.BaseDirectory;
        string messageTemplatePath = Path.Combine(appRoot, "HtmlTemplates", "message.html");
        var messageTemplate = File.ReadAllText(messageTemplatePath);

        var attachmentFileNames = attachments.Select(a =>
            GenerateHtml(
                "message-attachments.html",
                new KeyValuePair<string, string>("value", a.FileName).ToDictionary()));

        var dictionary = new Dictionary<string, string>
        {
            { "caseNumber",  caseNumber },
            { "title", caseTitle },
            { "messageNumber", messageNumber.ToString() ?? string.Empty },
            { "timestamp", createdAt.ToString("dd-MM-yyyy HH:mm:ss") },
            { "fromName", personName },
            { "fromEmail", personEmail },
            { "attachments", string.Join("", attachmentFileNames) },
            { "messageContent", content }
        };

        var html = messageTemplate.ReplacePlaceholders(dictionary);
        return html;
    }
}
