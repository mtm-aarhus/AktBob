namespace AktBob.Shared;

// Avoid untestable hardcoded delay time values!

public interface ITimeProvider
{
    Task Delay(int milliseconds, CancellationToken cancellationToken = default);
}

public class TimeProvider : ITimeProvider
{
    public Task Delay(int milliseconds, CancellationToken cancellationToken = default) => Delay(milliseconds, cancellationToken);
}
