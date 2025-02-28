using AktBob.Shared.Middlewares;
using Microsoft.Extensions.DependencyInjection;

namespace AktBob.Shared.CQRS;

public class QueryDispatcher : IQueryDispatcher
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IEnumerable<IMediatorMiddleware<IQuery<object>, object>> _middlewares;

    public QueryDispatcher(IServiceProvider serviceProvider, IEnumerable<IMediatorMiddleware<IQuery<object>, object>> middlewares)
    {
        _serviceProvider = serviceProvider;
        _middlewares = middlewares;
    }

    public async Task<TResponse> Dispatch<TResponse>(IQuery<TResponse> query, CancellationToken cancellationToken = default) where TResponse : class
    {
        var handlerType = typeof(IQueryHandler<,>).MakeGenericType(query.GetType(), typeof(TResponse));
        var handler = _serviceProvider.GetRequiredService(handlerType); // as IQueryHandler<IQuery<TResponse>, TResponse>;

        if (handler is null)
        {
            throw new InvalidOperationException($"No handler registered for query type {query.GetType().Name}");
        }

        Func<Task<TResponse>> pipeline = () => handler.Handle(query, cancellationToken);

        foreach (var middleware in _middlewares)
        {
            var middlewareType = typeof(IMediatorMiddleware<,>).MakeGenericType(query.GetType(), typeof(TResponse));
            var typedMiddleware = _serviceProvider.GetRequiredService(middlewareType) as IMediatorMiddleware<IQuery<TResponse>, TResponse>;

            var next = pipeline;
            pipeline = () => ((IMediatorMiddleware<IQuery<TResponse>, TResponse>)typedMiddleware).HandleAsync(query, next);
        }

        return await pipeline();
    }
}