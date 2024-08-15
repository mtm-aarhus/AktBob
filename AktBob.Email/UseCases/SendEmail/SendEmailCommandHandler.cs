using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Mail;

namespace AktBob.Email.UseCases.SendEmail;
internal class SendEmailCommandHandler : IRequestHandler<SendEmailCommand>
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SendEmailCommandHandler> _logger;

    public SendEmailCommandHandler(IConfiguration configuration, ILogger<SendEmailCommandHandler> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task Handle(SendEmailCommand request, CancellationToken cancellationToken)
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
