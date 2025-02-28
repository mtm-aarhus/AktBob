namespace AktBob.Shared.CQRS;

public interface ICommandHandler<in ICommand, TResponse>
{
    Task<TResponse> Handle(ICommand command, CancellationToken cancellationToken = default);
}

public interface ICommandHandler<in ICommand>
{
    Task Handle(ICommand command, CancellationToken cancellationToken = default);
}