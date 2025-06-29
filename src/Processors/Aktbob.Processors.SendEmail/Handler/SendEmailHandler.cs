using Aktbob.Processors.SendEmail.Client;
using AktBob.Shared;
using AktBob.Shared.Exceptions;
using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace Aktbob.Processors.SendEmail.Handler;
internal class SendEmailHandler(
    IAppConfig appConfig,
    ISmtpClient smtpClient,
    ILogger<SendEmailHandler> logger,
    string from,
    string smtp,
    int port) : ISendEmailHandler
{
    public void Handle(string to, string subject, string body, bool bodyIsHtml = false)
    {
        if (string.IsNullOrWhiteSpace(to))
        {
            throw new BusinessException("Email recipient is empty");
        }

        smtpClient.Connect(smtp, port);

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(from, from));
        message.To.Add(new MailboxAddress(to, to));
        message.Subject = subject;
        message.Body = new TextPart(bodyIsHtml ? "html" : "plain")
        {
            Text = body
        };

        smtpClient.Send(message);
        smtpClient.Disconnect(true);
        logger.LogInformation("Email sent to {recipient} with subject: {subject}", to, subject);
    }
}
