using Microsoft.Extensions.DependencyInjection;

namespace AktBob.Shared.CQRS;

public class QueryDispatcher : IQueryDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    public QueryDispatcher(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

    public Task<TResponse> Dispatch<TResponse>(IQuery<TResponse> query, CancellationToken cancellationToken = default) where TResponse : class
    {
        var handler = _serviceProvider.GetRequiredService<IQueryHandler<IQuery<TResponse>, TResponse>>();
        return handler.Handle(query, cancellationToken);
    }
}
