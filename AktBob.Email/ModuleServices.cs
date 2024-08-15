using Ardalis.GuardClauses;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AktBob.Email;

public static class ModuleServices
{
    public static IServiceCollection AddEmailModuleServices(
        this IServiceCollection services,
        IConfiguration configuration,
        List<System.Reflection.Assembly> mediatrAssemblies)
    {
        Guard.Against.Null(configuration.GetValue<int>("EmailModule:IntervalSeconds"));
        Guard.Against.NullOrEmpty(configuration.GetValue<string>("EmailModule:From"));
        Guard.Against.NullOrEmpty(configuration.GetValue<string>("EmailModule:Smtp"));

        // Queue
        services.AddTransient<IQueueService>(serviceProvider =>
        {
            var queueConnectionString = Guard.Against.NullOrEmpty(configuration.GetConnectionString("AzureStorage"));
            var queueName = Guard.Against.NullOrEmpty(configuration.GetValue<string>("EmailModule:QueueName"));
            var queueVisibilityTimeoutSeconds = configuration.GetValue<int?>("EmailModule:QueueVisibilityTimeoutSeconds") ?? 60;

            return new QueueService(queueConnectionString, queueName, queueVisibilityTimeoutSeconds);
        });

        services.AddHostedService<SendEmailBackgroundService>();

        // Mediatr
        mediatrAssemblies.Add(typeof(ModuleServices).Assembly);
        return services;
    }
}
