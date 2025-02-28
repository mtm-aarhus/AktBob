namespace AktBob.Shared.CQRS;

public interface ICommandDispatcher
{
    Task<TResponse> Dispatch<TResponse>(ICommand<TResponse> command, CancellationToken cancellationToken = default)
        where TResponse : class;

    Task Dispatch(ICommand command, CancellationToken cancellationToken = default);
}
