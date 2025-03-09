//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Logging;

//namespace AktBob.Shared;

//internal class JobHandlerFactory<TJob> : IJobHandler<TJob> where TJob : class
//{
//    private readonly IServiceProvider _serviceProvider;

//    public JobHandlerFactory(IServiceProvider serviceProvider)
//    {
//        _serviceProvider = serviceProvider;
//    }

//    public async Task Handle(TJob job, CancellationToken cancellationToken = default)
//    {
//        var inner = _serviceProvider.GetRequiredService<IJobHandler<TJob>>();
//        var logger = _serviceProvider.GetRequiredService<ILogger<JobHandlerDecoratorFactory<TJob>>>();

//        var loggingDecoratedHandler = new JobHandlerDecoratorFactory<TJob>(inner, logger);
//        await loggingDecoratedHandler.Handle(job, cancellationToken);
//    }
//}
