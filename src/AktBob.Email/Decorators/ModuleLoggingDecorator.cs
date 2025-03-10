using AktBob.Email.Contracts;
using Microsoft.Extensions.Logging;

namespace AktBob.Email.Decorators;

internal class ModuleLoggingDecorator(IEmailModule inner, ILogger<EmailModule> logger) : IEmailModule
{
    private readonly IEmailModule _inner = inner;
    private readonly ILogger<EmailModule> _logger = logger;

    public void Send(string to, string subject, string body)
    {
        _logger.LogInformation("Enqueueing job: Send email. To = {to}, Subject = {subject}, Body = {body}", to, subject, body);
        _inner.Send(to, subject, body);
    }
}
