using Microsoft.Extensions.Logging;

namespace AktBob.Email.Handler;

internal class SendEmailHandlerLogging : ISendEmailHandler
{
    private readonly ISendEmailHandler _next;
    private readonly ILogger<SendEmailHandler> _logger;

    public SendEmailHandlerLogging(ISendEmailHandler next, ILogger<SendEmailHandler> logger)
    {
        _next = next;
        _logger = logger;
    }

    public void Handle(string to, string subject, string body, bool bodyIsHtml = false)
    {
        _logger.LogInformation("Enqueueing job: Send email. To = {to}, Subject = {subject}, Body = {body}", to, subject, body);
        _next.Handle(to, subject, body, bodyIsHtml);
    }
}