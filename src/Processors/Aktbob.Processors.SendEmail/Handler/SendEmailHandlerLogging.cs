using Microsoft.Extensions.Logging;

namespace Aktbob.Processors.SendEmail.Handler;

internal class SendEmailHandlerLogging(ISendEmailHandler next, ILogger<SendEmailHandler> logger) : ISendEmailHandler
{
    public void Handle(string to, string subject, string body, bool bodyIsHtml = false)
    {
        logger.LogInformation("Enqueueing job: Send email. To = {to}, Subject = {subject}, Body = {body}", to, subject, body);
        next.Handle(to, subject, body, bodyIsHtml);
    }
}