namespace AktBob.Shared.CQRS;

public interface IQueryDispatcher
{
    Task<TResponse> Dispatch<TResponse>(IQuery<TResponse> query, CancellationToken cancellationToken = default)
        where TResponse : class;
}
