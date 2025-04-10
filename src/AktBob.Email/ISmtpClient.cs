using MimeKit;

namespace AktBob.Email;
internal interface ISmtpClient : IDisposable
{
    void Connect(string host, int port, bool useSsl);
    Task Send(MimeMessage message);
    void Disconnect(bool quit);
}
