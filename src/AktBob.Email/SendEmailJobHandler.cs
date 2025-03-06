using AktBob.Shared;
using Ardalis.GuardClauses;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using MimeKit;
using System.Text;

namespace AktBob.Email;

internal record SendEmailJob(string To, string Base64Subject, string Base64Body);
internal class SendEmailJobHandler(IConfiguration configuration) : IJobHandler<SendEmailJob>
{
    private readonly IConfiguration _configuration = configuration;

    public async Task Handle(SendEmailJob job, CancellationToken cancellationToken = default)
    {
        var subject = Encoding.UTF8.GetString(Convert.FromBase64String(job.Base64Subject));
        var body = Encoding.UTF8.GetString(Convert.FromBase64String(job.Base64Body));

        var smtpUrl = Guard.Against.NullOrEmpty(_configuration.GetValue<string>("EmailModule:SmtpUrl"));
        var smtpPort = _configuration.GetValue<int>("EmailModule:SmtpPort");
        var smtpUseSsl = _configuration.GetValue<bool?>("EmailModule:SmtpUseSsl") ?? false;
        var from = Guard.Against.NullOrEmpty(_configuration.GetValue<string>("EmailModule:From"));

        using (SmtpClient smtpClient = new SmtpClient())
        {
            smtpClient.Connect(smtpUrl, smtpPort, smtpUseSsl);

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(from, from));
            message.To.Add(new MailboxAddress(job.To, job.To));
            message.Subject = subject;
            message.Body = new TextPart("plain")
            {
                Text = body
            };

            await smtpClient.SendAsync(message);

            smtpClient.Disconnect(true);
        }
    }
}
