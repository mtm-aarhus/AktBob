using System.Text;
using AktBob.Email.Contracts;
using AktBob.Email.Handler;
using AktBob.Shared;

namespace AktBob.Email.Jobs;

internal class SendEmailJobHandler(ISendEmailHandler sendEmailHandler) : IJobHandler<SendEmailJob>
{
    private readonly ISendEmailHandler _sendEmailHandler = sendEmailHandler;

    public Task Handle(SendEmailJob job, CancellationToken cancellationToken = default)
    {
        var subject = Encoding.UTF8.GetString(Convert.FromBase64String(job.Base64Subject));
        var body = Encoding.UTF8.GetString(Convert.FromBase64String(job.Base64Body));
        _sendEmailHandler.Handle(job.To, subject, body, job.bodyIsHtml);
        return Task.CompletedTask;
    }
}
