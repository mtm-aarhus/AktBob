using AAK.Podio;
using AktBob.Podio.Contracts;
using AktBob.Podio.Decorators;
using AktBob.Podio.Handlers;
using AktBob.Podio.Jobs;
using AktBob.Shared;
using Ardalis.GuardClauses;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AktBob.Podio;

public static class ModuleServices
{
    public static IServiceCollection AddPodioModule(this IServiceCollection services, IConfiguration configuration)
    {
        var podioAppTokens = Guard.Against.NullOrEmpty(configuration.GetSection("Podio:AppTokens").GetChildren().ToDictionary(x => x.Key, x => x.Value));
        services.AddPodioFactory(new Uri(Guard.Against.NullOrEmpty(configuration.GetValue<string>("Podio:BaseAddress"))));

        // Handlers
        services.AddScoped<IGetItemHandler, GetItemHandler>();
        services.AddScoped<IPostCommentHandler, PostCommentHandler>();
        services.AddScoped<IUpdateTextFieldHandler, UpdateTextFieldHandler>();

        // Jobs
        services.AddScoped<IJobHandler<UpdateTextFieldJob>, UpdateTextField>();
        services.AddScoped<IJobHandler<PostCommentJob>, PostComment>();

        // Module service orchestrator
        services.AddScoped<IPodioModule>(provider =>
        {
            var inner = new Module(
                provider.GetRequiredService<IJobDispatcher>(),
                provider.GetRequiredService<IGetItemHandler>());

            var withLogging = new ModuleLoggingDecorator(
                inner,
                provider.GetRequiredService<ILogger<ModuleLoggingDecorator>>());

            var withExceptionHandling = new ModuleExceptionDecorator(
                withLogging,
                provider.GetRequiredService<ILogger<ModuleExceptionDecorator>>());

            return withExceptionHandling;
        });

        return services;
    }
}
