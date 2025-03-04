using AktBob.Email.Contracts;
using AktBob.Shared;

namespace AktBob.Email;

internal class EmailModule(IJobDispatcher jobDispatcher) : IEmailModule
{
    public void Send(string to, string subject, string body) => jobDispatcher.Dispatch(new SendEmailJob(to, subject, body));
}
