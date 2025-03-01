namespace AktBob.Shared;

// Avoid untestable hardcoded delay time values!

public interface ITimeProvider
{
    Task Delay(TimeSpan timeSpan, CancellationToken cancellationToken = default);
}

public class TimeProvider : ITimeProvider
{
    public async Task Delay(TimeSpan timeSpan, CancellationToken cancellationToken = default)
    {
        await Task.Delay(timeSpan, cancellationToken);
    }
}
