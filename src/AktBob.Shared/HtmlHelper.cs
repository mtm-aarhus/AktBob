using AktBob.Shared.Contracts.Modules.Deskpro.DTOs;
using AktBob.Shared.Extensions;

namespace AktBob.Shared;
public static class HtmlHelper
{
    public static string GenerateHtml(Dictionary<string, string> fields, string templatePath)
    {
        string appRoot = AppDomain.CurrentDomain.BaseDirectory;
        var template = File.ReadAllText(Path.Combine(appRoot, templatePath)); // TODO: cache
        var html = template.ReplacePlaceholders(fields);
        return html;
    }

    public static IEnumerable<string> GenerateListOfFieldValues(int[] fieldIds, TicketDto ticketDto, string templatePath)
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
            var html = GenerateHtml(kvp.ToDictionary(), templatePath);
            items.Add(html);
        }

        return items;
    }

    public static string GenerateMessageHtml(bool isAgentNote, DateTime createdAt, string personName, string personEmail, string recipientName, string recipientEmail, string content, string caseNumber, string caseTitle, int messageNumber, IEnumerable<AttachmentDto> attachments)
    {
        string appRoot = AppDomain.CurrentDomain.BaseDirectory;
        var template = isAgentNote ? "message-agent-note.html" : "message.html";
        string messageTemplatePath = "HtmlTemplates/" + template;

        var attachmentFileNames = attachments.Select(a =>
            GenerateHtml(
                new KeyValuePair<string, string>("value", a.FileName).ToDictionary(),
                "HtmlTemplates/message-attachments.html"));

        var dictionary = new Dictionary<string, string>
        {
            { "caseNumber",  caseNumber },
            { "title", caseTitle },
            { "messageNumber", messageNumber.ToString() ?? string.Empty },
            { "timestamp", createdAt.ToString("dd-MM-yyyy HH:mm:ss") },
            { "fromName", personName },
            { "fromEmail", personEmail },
            { "toName", recipientName },
            { "toEmail", recipientEmail },
            { "attachments", string.Join("", attachmentFileNames) },
            { "messageContent", content }
        };

        var html = GenerateHtml(dictionary, messageTemplatePath);
        return html;
    }
}
