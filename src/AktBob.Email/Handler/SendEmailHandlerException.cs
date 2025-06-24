using Microsoft.Extensions.Logging;

namespace AktBob.Email.Handler;

internal class SendEmailHandlerException(ISendEmailHandler next, ILogger<SendEmailHandler> logger) : ISendEmailHandler
{
    public void Handle(string to, string subject, string body, bool bodyIsHtml = false)
    {
        try
        {
            next.Handle(to, subject, body, bodyIsHtml);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in {name}", nameof(SendEmailHandler));
        }
    }
}