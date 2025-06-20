using ErrorOr;

namespace AktBob.Shared;

public interface IMessageBus
{
    Task<ErrorOr<Success>> SendMessage(string queue, object? payload, CancellationToken cancellationToken = default);
    Task<ErrorOr<Success>> ScheduleMessage(string queue, object? payload, DateTimeOffset offset, CancellationToken cancellationToken = default);

    Task<ErrorOr<Success>> SendMessages(string queue, object[]? payloads, CancellationToken cancellationToken = default);
}