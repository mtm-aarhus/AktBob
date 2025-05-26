using Microsoft.Extensions.Logging;

namespace AktBob.Email.Handler;

internal class SendEmailHandlerException : ISendEmailHandler
{
    private readonly ISendEmailHandler _next;
    private readonly ILogger<SendEmailHandler> _logger;

    public SendEmailHandlerException(ISendEmailHandler next, ILogger<SendEmailHandler> logger)
    {
        _next = next;
        _logger = logger;
    }
    public void Handle(string to, string subject, string body, bool bodyIsHtml = false)
    {
        try
        {
            _next.Handle(to, subject, body, bodyIsHtml);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {name}", nameof(SendEmailHandler));
        }
    }
}