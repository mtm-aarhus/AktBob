using Microsoft.Extensions.Logging;

namespace AktBob.Shared.Middlewares;

internal class LoggingMiddleware<TRequest, TResponse> : IMediatorMiddleware<TRequest, TResponse>
{
    private readonly ILogger<LoggingMiddleware<TRequest, TResponse>> _logger;

    public LoggingMiddleware(ILogger<LoggingMiddleware<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> HandleAsync(TRequest request, Func<Task<TResponse>> next)
    {
        var requestType = request!.GetType().Name;
        _logger.LogInformation("Invoking {requestType}: {request}", requestType, request);

        var response = await next();

        _logger.LogInformation("Handled {requestType}: {request}", requestType, request);
        return response;
    }
}

internal class LoggingMiddleware<TRequest> : IMediatorMiddleware<TRequest>
{
    private readonly ILogger<LoggingMiddleware<TRequest>> _logger;

    public LoggingMiddleware(ILogger<LoggingMiddleware<TRequest>> logger)
    {
        _logger = logger;
    }

    public async Task HandleAsync(TRequest request, Func<Task> next)
    {
        var requestType = request!.GetType().Name;
        _logger.LogInformation("Invoking {requestType}: {request}", requestType, request);

        await next();

        _logger.LogInformation("Handled {requestType}: {request}", requestType, request);
    }
}
