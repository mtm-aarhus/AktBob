using AktBob.Email.Client;
using AktBob.Shared;
using AktBob.Shared.Exceptions;
using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace AktBob.Email.Handler;
internal class SendEmailHandler(
    IAppConfig appConfig,
    ISmtpClient smtpClient,
    ILogger<SendEmailHandler> logger) : ISendEmailHandler
{
    private readonly string _smtpUrl = Guard.Against.NullOrEmpty(appConfig.GetValue<string>("EmailModule:SmtpUrl"));
    private readonly int _smtpPort = Guard.Against.Null(appConfig.GetValue<int?>("EmailModule:SmtpPort"));
    private readonly string _from = Guard.Against.NullOrEmpty(appConfig.GetValue<string>("EmailModule:From"));

    public void Handle(string to, string subject, string body, bool bodyIsHtml = false)
    {
        if (string.IsNullOrWhiteSpace(to))
        {
            throw new BusinessException("Email recipient is empty");
        }

        smtpClient.Connect(_smtpUrl, _smtpPort);

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_from, _from));
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
