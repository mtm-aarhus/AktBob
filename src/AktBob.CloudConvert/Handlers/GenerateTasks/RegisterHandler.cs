using Microsoft.Extensions.DependencyInjection;

namespace AktBob.CloudConvert.Handlers.GenerateTasks;

internal static class RegisterHandler
{
    public static IServiceCollection AddGenerateTasksHandler(this IServiceCollection services)
    {
        services.AddScoped<IGenerateTasksHandler>(provider =>
        {
            var logger = provider.GetRequiredService<ILogger<GenerateTasksHandler>>();

            var inner = new GenerateTasksHandler();
            var withLogging = new GenerateTasksHandlerLogging(inner, logger);
            var withExceptionHandling = new GenerateTasksHandlerException(withLogging, logger);

            return withExceptionHandling;
        });

        return services;
    }
}
