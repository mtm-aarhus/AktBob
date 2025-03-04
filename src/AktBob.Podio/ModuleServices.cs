using AAK.Podio;
using AktBob.Podio.Contracts;
using AktBob.Podio.Handlers;
using AktBob.Podio.Jobs;
using AktBob.Shared;
using Ardalis.GuardClauses;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AktBob.Podio;

public static class ModuleServices
{
    public static IServiceCollection AddPodioModule(this IServiceCollection services, IConfiguration configuration)
    {
        var podioAppTokens = Guard.Against.NullOrEmpty(configuration.GetSection("Podio:AppTokens").GetChildren().ToDictionary(x => x.Key, x => x.Value));
        services.AddPodioFactory(new Uri(Guard.Against.NullOrEmpty(configuration.GetValue<string>("Podio:BaseAddress"))));

        // Handlers
        services.AddTransient<IGetItemHandler, GetItemHandler>();
        services.AddTransient<IPostCommentHandler, PostCommentHandler>();
        services.AddTransient<IUpdateTextFieldHandler, UpdateTextFieldHandler>();

        // Jobs
        services.AddScoped<IJobHandler<UpdateTextFieldJob>, UpdateTextField>();
        services.AddScoped<IJobHandler<PostCommentJob>, PostComment>();

        // Module service orchestrator
        services.AddScoped<IPodioModule, Module>();
        return services;
    }
}
