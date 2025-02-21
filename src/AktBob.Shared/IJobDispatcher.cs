namespace AktBob.Shared;
public interface IJobDispatcher
{
    void Dispatch<TJob>(TJob job) where TJob : class;
    void Dispatch<TJob>(TJob job, TimeSpan delay) where TJob : class;
}