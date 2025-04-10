using AktBob.Shared;
using System.Text;

namespace AktBob.Email;

internal record SendEmailJob(string To, string Base64Subject, string Base64Body, bool bodyIsHtml = false);
internal class SendEmailJobHandler(IEmail email) : IJobHandler<SendEmailJob>
{
    private readonly IEmail _email = email;

    public Task Handle(SendEmailJob job, CancellationToken cancellationToken = default)
    {
        var subject = Encoding.UTF8.GetString(Convert.FromBase64String(job.Base64Subject));
        var body = Encoding.UTF8.GetString(Convert.FromBase64String(job.Base64Body));
        _email.Send(job.To, subject, body, job.bodyIsHtml);
        return Task.CompletedTask;
    }
}
