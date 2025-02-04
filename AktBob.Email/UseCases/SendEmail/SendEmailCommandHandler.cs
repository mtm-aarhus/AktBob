using MassTransit.Mediator;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Mail;

namespace AktBob.Email.UseCases.SendEmail;
public class SendEmailCommandHandler(IConfiguration configuration, ILogger<SendEmailCommandHandler> logger) : MediatorRequestHandler<SendEmailCommand>
{
    private readonly IConfiguration _configuration = configuration;
    private readonly ILogger<SendEmailCommandHandler> _logger = logger;

    protected override async Task Handle(SendEmailCommand request, CancellationToken cancellationToken)
    {
        var from = _configuration.GetValue<string>("EmailModule:From");
        var smtp = _configuration.GetValue<string>("EmailModule:Smtp");

        var smtpClient = new SmtpClient(smtp);
        var mailMessage = new MailMessage();
        mailMessage.From = new MailAddress(from!);
        mailMessage.To.Add(request.To);
        mailMessage.Subject = request.Subject;
        mailMessage.Body = request.Body;
        mailMessage.IsBodyHtml = request.IsBodyHtml;

        await smtpClient.SendMailAsync(mailMessage, cancellationToken);

        _logger.LogInformation($"Email sent. To: '{request.To}' Subject: '{request.Subject}' Body: '{request.Body}'");
    }
}
