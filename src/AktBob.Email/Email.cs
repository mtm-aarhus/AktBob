using AktBob.Shared;
using AktBob.Shared.Exceptions;
using Ardalis.GuardClauses;
using MimeKit;

namespace AktBob.Email;
internal class Email : IEmail
{
    private readonly IAppConfig _appConfig;
    private readonly ISmtpClient _smtpClient;
    private readonly string _smtpUrl;
    private readonly int _smtpPort;
    private readonly bool _smtpUseSsl;
    private readonly string _from;

    public Email(IAppConfig appConfig, ISmtpClient smtpClient)
    {
        _appConfig = appConfig;
        _smtpClient = smtpClient;
        _smtpUrl = Guard.Against.NullOrEmpty(_appConfig.GetValue<string>("EmailModule:SmtpUrl"));
        _smtpPort = _appConfig.GetValue<int>("EmailModule:SmtpPort");
        _smtpUseSsl = _appConfig.GetValue<bool?>("EmailModule:SmtpUseSsl") ?? false;
        _from = Guard.Against.NullOrEmpty(_appConfig.GetValue<string>("EmailModule:From"));
    }

    public async Task Send(string to, string subject, string body)
    {
        if (string.IsNullOrWhiteSpace(to))
        {
            throw new BusinessException("Email recipient is empty");
        }

        _smtpClient.Connect(_smtpUrl, _smtpPort, _smtpUseSsl);

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_from, _from));
        message.To.Add(new MailboxAddress(to, to));
        message.Subject = subject;
        message.Body = new TextPart("plain")
        {
            Text = body
        };

        await _smtpClient.Send(message);
        _smtpClient.Disconnect(true);
    }
}
