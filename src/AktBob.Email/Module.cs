using AktBob.Email.Contracts;
using AktBob.Shared;
using System.Text;

namespace AktBob.Email;

internal class Module(IJobDispatcher jobDispatcher) : IEmailModule
{
    public void Send(string to, string subject, string body)
    {
        var subjectBytes = Encoding.UTF8.GetBytes(subject);
        var base64Subject = Convert.ToBase64String(subjectBytes);

        var bodyBytes = Encoding.UTF8.GetBytes(body);
        var base64Body = Convert.ToBase64String(bodyBytes);

        jobDispatcher.Dispatch(new SendEmailJob(to, base64Subject, base64Body));
    }
}
