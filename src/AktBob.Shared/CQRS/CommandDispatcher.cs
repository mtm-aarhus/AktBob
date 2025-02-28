using Microsoft.Extensions.DependencyInjection;

namespace AktBob.Shared.CQRS;

public class CommandDispatcher : ICommandDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    public CommandDispatcher(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

    public Task Dispatch(ICommand command, CancellationToken cancellationToken = default)
    {
        var handler = _serviceProvider.GetRequiredService<ICommandHandler<ICommand>>();
        handler.Handle(command, cancellationToken);
        return Task.CompletedTask;
    }

    public Task<TResponse> Dispatch<TResponse>(ICommand<TResponse> command, CancellationToken cancellationToken = default) where TResponse : class
    {
        var handler = _serviceProvider.GetRequiredService<ICommandHandler<ICommand<TResponse>, TResponse>>();
        return handler.Handle(command, cancellationToken);
    }
}
