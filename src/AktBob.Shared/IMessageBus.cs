using ErrorOr;

namespace AktBob.Shared;

public interface IMessageBus
{
    Task<ErrorOr<Success>> SendMessage(string queue, object? payload, CancellationToken cancellationToken = default);
}