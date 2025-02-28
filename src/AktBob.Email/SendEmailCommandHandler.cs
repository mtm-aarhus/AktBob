using AktBob.Email.Contracts;
using AktBob.Shared.CQRS;
using Microsoft.Extensions.Configuration;
using System.Net.Mail;

namespace AktBob.Email;
internal class SendEmailCommandHandler(IConfiguration configuration) : ICommandHandler<SendEmailCommand>
{
    private readonly IConfiguration _configuration = configuration;

    public async Task Handle(SendEmailCommand request, CancellationToken cancellationToken)
    {
        var from = _configuration.GetValue<string>("EmailModule:From");
        var smtp = _configuration.GetValue<string>("EmailModule:Smtp");

        if (!string.IsNullOrEmpty(from)
            && !string.IsNullOrEmpty(request.To)
            && !string.IsNullOrEmpty(smtp))
        {
            var smtpClient = new SmtpClient(smtp);
            var mailMessage = new MailMessage();
            mailMessage.From = new MailAddress(from!);
            mailMessage.To.Add(request.To);
            mailMessage.Subject = request.Subject;
            mailMessage.Body = request.Body;
            mailMessage.IsBodyHtml = request.IsBodyHtml;

            await smtpClient.SendMailAsync(mailMessage, cancellationToken);
        }
    }
}
