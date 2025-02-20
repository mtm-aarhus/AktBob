using AktBob.Email.Contracts;
using MassTransit.Mediator;
using Microsoft.Extensions.Configuration;
using System.Net.Mail;

namespace AktBob.Email;
public class SendEmailCommandHandler(IConfiguration configuration) : MediatorRequestHandler<SendEmailCommand>
{
    private readonly IConfiguration _configuration = configuration;

    protected override async Task Handle(SendEmailCommand request, CancellationToken cancellationToken)
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
