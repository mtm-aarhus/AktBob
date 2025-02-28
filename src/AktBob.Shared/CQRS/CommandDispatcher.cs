using AktBob.Shared.Middlewares;
using Microsoft.Extensions.DependencyInjection;

namespace AktBob.Shared.CQRS;

public class CommandDispatcher : ICommandDispatcher
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IEnumerable<IMediatorMiddleware<ICommand<object>, object>> _middlewaresWithResponse;
    private readonly IEnumerable<IMediatorMiddleware<ICommand>> _middlewaresWithoutResponse;

    public CommandDispatcher(IServiceProvider serviceProvider, IEnumerable<IMediatorMiddleware<ICommand<object>, object>> middlewaresWithResponse, IEnumerable<IMediatorMiddleware<ICommand>> middlewaresWithoutResponse)
    {
        _serviceProvider = serviceProvider;
        _middlewaresWithResponse = middlewaresWithResponse;
        _middlewaresWithoutResponse = middlewaresWithoutResponse;
    }

    public async Task Dispatch(ICommand command, CancellationToken cancellationToken = default)
    {
        var handler = _serviceProvider.GetRequiredService<ICommandHandler<ICommand>>();

        // Middleware execution pipeline
        Func<Task> pipeline = () => handler.Handle(command, cancellationToken);

        foreach (var middleware in _middlewaresWithoutResponse.Reverse())
        {
            var next = pipeline;
            pipeline = () => middleware.HandleAsync(command, next);
        }

        await pipeline();
    }

    public async Task<TResponse> Dispatch<TResponse>(ICommand<TResponse> command, CancellationToken cancellationToken = default) where TResponse : class
    {
        var handler = _serviceProvider.GetRequiredService<ICommandHandler<ICommand<TResponse>, TResponse>>();

        Func<Task<TResponse>> pipeline = () => handler.Handle(command, cancellationToken);

        foreach (var middleware in _middlewaresWithResponse.Reverse())
        {
            var next = pipeline;
            pipeline = () => ((IMediatorMiddleware<ICommand<TResponse>, TResponse>)middleware).HandleAsync(command, next);
        }

        return await pipeline();
    }
}
