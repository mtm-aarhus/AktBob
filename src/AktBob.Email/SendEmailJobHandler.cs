using AktBob.Shared;
using Ardalis.GuardClauses;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace AktBob.Email;

internal record SendEmailJob(string To, string Subject, string Body);
internal class SendEmailJobHandler(IConfiguration configuration) : IJobHandler<SendEmailJob>
{
    private readonly IConfiguration _configuration = configuration;

    public async Task Handle(SendEmailJob job, CancellationToken cancellationToken = default)
    {
        var smtpUrl = Guard.Against.NullOrEmpty(_configuration.GetValue<string>("EmailModule:SmtpUrl"));
        var smtpPort = _configuration.GetValue<int>("EmailModule:SmtpPort");
        var smtpUseSsl = _configuration.GetValue<bool?>("EmailModule:SmtpUseSsl") ?? false;
        var from = Guard.Against.NullOrEmpty(_configuration.GetValue<string>("EmailModule:From"));

        using (SmtpClient smtpClient = new SmtpClient())
        {
            smtpClient.Connect(smtpUrl, 25, false);

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(from, from));
            message.To.Add(new MailboxAddress(job.To, job.To));
            message.Subject = job.Subject;
            message.Body = new TextPart("plain")
            {
                Text = job.Body
            };

            await smtpClient.SendAsync(message);

            smtpClient.Disconnect(true);
        }
    }
}
