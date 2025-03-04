using AktBob.Email.Contracts;
using AktBob.Shared;
using Ardalis.GuardClauses;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AktBob.Email;

public static class ModuleServices
{
    public static IServiceCollection AddEmailModuleServices(this IServiceCollection services, IConfiguration configuration)
    {
        Guard.Against.NullOrEmpty(configuration.GetValue<string>("EmailModule:From"));
        Guard.Against.NullOrEmpty(configuration.GetValue<string>("EmailModule:Smtp"));

        services.AddScoped<IJobHandler<SendEmailJob>, SendEmailJobHandler>();
        services.AddScoped<IEmailModule, EmailModule>();

        return services;
    }
}
