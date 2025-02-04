namespace AktBob.Shared;
public interface IJobHandler<TJob> where TJob : class
{
    Task Handle(TJob job);
}
