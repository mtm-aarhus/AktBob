using AktBob.Email.Contracts;
using Microsoft.Extensions.Logging;

namespace AktBob.Email;

internal class ModuleExceptionDecorator(IEmailModule inner, ILogger<ModuleExceptionDecorator> logger) : IEmailModule
{
    private readonly IEmailModule _inner = inner;
    private readonly ILogger<ModuleExceptionDecorator> _logger = logger;

    public void Send(string to, string subject, string body)
    {
		try
		{
			_inner.Send(to, subject, body);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error in {name}", nameof(Send));
			throw;
		}
    }
}
