using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace AktBob.Shared;

internal class JobHandlerDecoratorFactory<TJob> : IJobHandler<TJob> where TJob : class
{
    private readonly IJobHandler<TJob> _inner;
    private readonly ILogger<IJobHandler<TJob>> _logger;

    public JobHandlerDecoratorFactory(IJobHandler<TJob> inner, ILogger<JobHandlerDecoratorFactory<TJob>> logger)
    {
        _inner = inner;
        _logger = logger;
    }

    public async Task Handle(TJob job, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting job {name} ({job})", typeof(TJob).Name, job);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            await _inner.Handle(job, cancellationToken);
            stopwatch.Stop();
            _logger.LogInformation("Finished job {name} in {duration} ms ({job})", typeof(TJob).Name, stopwatch.ElapsedMilliseconds, job);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Job {name} failed after {duration} ms: {job}", typeof(TJob).Name, stopwatch.ElapsedMilliseconds, job);
            throw;
        }
    }
}