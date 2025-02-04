namespace AktBob.Shared;
public interface IJobDispatcher
{
    void Dispatch<TJob>(TJob job) where TJob : class;
}
