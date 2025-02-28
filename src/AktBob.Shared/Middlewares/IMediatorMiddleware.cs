namespace AktBob.Shared.Middlewares;

public interface IMediatorMiddleware<TRequest, TResponse>
{
    Task<TResponse> HandleAsync(TRequest request, Func<Task<TResponse>> next);
}

public interface IMediatorMiddleware<TRequest>
{
    Task HandleAsync(TRequest request, Func<Task> next);
}

