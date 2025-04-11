using AktBob.Shared;
using AktBob.Shared.Exceptions;
using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace AktBob.Email;
internal class Email : IEmail
{
    private readonly IAppConfig _appConfig;
    private readonly ISmtpClient _smtpClient;
    private readonly ILogger<Email> _logger;
    private readonly string _smtpUrl;
    private readonly int _smtpPort;
    private readonly string _from;

    public Email(IAppConfig appConfig, ISmtpClient smtpClient, ILogger<Email> logger)
    {
        _appConfig = appConfig;
        _smtpClient = smtpClient;
        _logger = logger;
        _smtpUrl = Guard.Against.NullOrEmpty(_appConfig.GetValue<string>("EmailModule:SmtpUrl"));
        _smtpPort = _appConfig.GetValue<int>("EmailModule:SmtpPort");
        _from = Guard.Against.NullOrEmpty(_appConfig.GetValue<string>("EmailModule:From"));
    }

    public void Send(string to, string subject, string body, bool bodyIsHtml = false)
    {
        if (string.IsNullOrWhiteSpace(to))
        {
            throw new BusinessException("Email recipient is empty");
        }


        _smtpClient.Connect(_smtpUrl, _smtpPort);

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_from, _from));
        message.To.Add(new MailboxAddress(to, to));
        message.Subject = subject;
        message.Body = new TextPart(bodyIsHtml ? "html" : "plain")
        {
            Text = body
        };

        _smtpClient.Send(message);
        _smtpClient.Disconnect(true);
        _logger.LogInformation("Email sent to {recipient} with subject: {subject}", to, subject);
    }
}
