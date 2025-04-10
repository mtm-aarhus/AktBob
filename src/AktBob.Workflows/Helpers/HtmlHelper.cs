﻿using AktBob.Deskpro.Contracts.DTOs;
using AktBob.Workflows.Extensions;
using AktBob.Shared.Extensions;

namespace AktBob.Workflows.Helpers;
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

    public static string GenerateMessageHtml(bool isAgentNote, DateTime createdAt, string personName, string personEmail, string recipientName, string recipientEmail, string content, string caseNumber, string caseTitle, int messageNumber, IEnumerable<AttachmentDto> attachments)
    {
        string appRoot = AppDomain.CurrentDomain.BaseDirectory;
        var template = isAgentNote ? "message-agent-note.html" : "message.html";
        string messageTemplatePath = Path.Combine(appRoot, "HtmlTemplates", template);
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
            { "toName", recipientName },
            { "toEmail", recipientEmail },
            { "attachments", string.Join("", attachmentFileNames) },
            { "messageContent", content }
        };

        var html = messageTemplate.ReplacePlaceholders(dictionary);
        return html;
    }
}
