using AktBob.Email.Contracts;
using Microsoft.Extensions.Configuration;
using System.Net.Mail;

namespace AktBob.Email;
internal class SendEmailHandler(IConfiguration configuration) : ISendEmailHandler
{
    private readonly IConfiguration _configuration = configuration;

    public async Task Handle(string to, string subject, string body, bool isBodyHtml, CancellationToken cancellationToken)
    {
        var from = _configuration.GetValue<string>("EmailModule:From");
        var smtp = _configuration.GetValue<string>("EmailModule:Smtp");

        if (!string.IsNullOrEmpty(from)
            && !string.IsNullOrEmpty(to)
            && !string.IsNullOrEmpty(smtp))
        {
            var smtpClient = new SmtpClient(smtp);
            var mailMessage = new MailMessage();
            mailMessage.From = new MailAddress(from!);
            mailMessage.To.Add(to);
            mailMessage.Subject = subject;
            mailMessage.Body = body;
            mailMessage.IsBodyHtml = isBodyHtml;

            await smtpClient.SendMailAsync(mailMessage, cancellationToken);
        }
    }
}
