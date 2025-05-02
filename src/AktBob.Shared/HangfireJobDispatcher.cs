using Hangfire;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Text.Json;

namespace AktBob.Shared;

public class HangfireJobDispatcher(IBackgroundJobClient backgroundJobClient) : IJobDispatcher
{
    private readonly IBackgroundJobClient _backgroundJobClient = backgroundJobClient;

    public void Dispatch<TJob>(TJob job) where TJob : class
    {
        _backgroundJobClient.Enqueue<IJobHandler<TJob>>(handler => handler.Handle(job, CancellationToken.None));
    }

    public void Dispatch<TJob>(TJob job, TimeSpan delay) where TJob : class
    {
        _backgroundJobClient.Schedule<IJobHandler<TJob>>(handler => handler.Handle(job, CancellationToken.None), delay);
    }

    public void Dispatch<TJob>(TJob job, DateTimeOffset offset) where TJob : class
    {
        _backgroundJobClient.Schedule<IJobHandler<TJob>>(handler => handler.Handle(job, CancellationToken.None), offset);
    }

    public bool IsJobAlreadyScheduled<T>(Type jobType, T identifierValue, string identifierName)
    {
        var monitoringApi = JobStorage.Current.GetMonitoringApi();
        var scheduledJobs = monitoringApi.ScheduledJobs(0, 1000); // Paging if needed

        foreach (var job in scheduledJobs)
        {

            if (job.Value == null || job.Value.Job.Type != typeof(IJobHandler<>).MakeGenericType(jobType))
            {
                continue;
            }

            var genericArg = job.Value.Job.Type.GenericTypeArguments.FirstOrDefault();
            if (genericArg != jobType)
            {
                continue;
            }

            if (job.Value.Job.Args == null || job.Value.Job.Args.Count == 0)
            {
                continue;
            }
            
            Debug.Assert(job.Value.Job.Args.Count == 1 && job.Value.Job.Args[0]?.GetType() == jobType);

            var jobArg = job.Value.Job.Args.FirstOrDefault(arg => arg?.GetType() == jobType);
            if (jobArg == null)
            {
                continue;
            }

            var property = jobType.GetProperty(identifierName);
            if (property == null)
            {
                continue;
            }

            var value = property.GetValue(jobArg);
            if (value != null && value.Equals(identifierValue))
            {
                return true;
            }
        }

        return false;
    }
}
