using MimeKit;

namespace Aktbob.Processors.SendEmail.Client;
internal interface ISmtpClient : IDisposable
{
    void Connect(string host, int port);
    void Send(MimeMessage message);
    void Disconnect(bool quit);
}
