using MailKit.Net.Smtp;
using MimeKit;

namespace AktBob.Email;
internal class SmtpClientWrapper : ISmtpClient
{
    private readonly SmtpClient _smtpClient = new SmtpClient();

    public void Connect(string host, int port, bool useSsl)
    {
        _smtpClient.Connect(host, port, useSsl);
    }

    public void Disconnect(bool quit)
    {
        _smtpClient.Disconnect(quit);
    }

    public void Dispose()
    {
        _smtpClient.Dispose();
    }

    public async Task Send(MimeMessage message)
    {
        await _smtpClient.SendAsync(message);
    }
}
