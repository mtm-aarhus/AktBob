using AktBob.Email.Contracts;
using Microsoft.Extensions.Logging;

namespace AktBob.Email.Decorators;

internal class ModuleExceptionDecorator(IEmailModule inner, ILogger<EmailModule> logger) : IEmailModule
{
    private readonly IEmailModule _inner = inner;
    private readonly ILogger<EmailModule> _logger = logger;

    public void Send(string to, string subject, string body, bool bodyIsHtml = false)
    {
		try
		{
			_inner.Send(to, subject, body, bodyIsHtml);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error in {name}", nameof(Send));
			throw;
		}
    }
}
