using MailKit.Net.Smtp;
using MimeKit;

namespace AktBob.Email;
internal class SmtpClientWrapper : ISmtpClient
{
    private readonly SmtpClient _smtpClient = new SmtpClient();

    public void Connect(string host, int port)
    {
        _smtpClient.Connect(host, port, MailKit.Security.SecureSocketOptions.None);
        _smtpClient.ServerCertificateValidationCallback = (s, c, h, e) => true;
    }

    public void Disconnect(bool quit)
    {
        _smtpClient.Disconnect(quit);
    }

    public void Dispose()
    {
        _smtpClient.Dispose();
    }

    public void Send(MimeMessage message)
    {
        _smtpClient.Send(message);
    }
}
