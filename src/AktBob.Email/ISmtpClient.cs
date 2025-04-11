using MimeKit;

namespace AktBob.Email;
internal interface ISmtpClient : IDisposable
{
    void Connect(string host, int port);
    void Send(MimeMessage message);
    void Disconnect(bool quit);
}
