using AAK.Deskpro;
using Ardalis.GuardClauses;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace AktBob.Deskpro;

public static class ModuleServices
{
    public static IServiceCollection AddDeskproModule(this IServiceCollection services, IConfiguration configuration, List<Assembly> mediatRAssemblies)
    {
        var deskproOptions = new DeskproOptions
        {
            BaseAddress = Guard.Against.NullOrEmpty(configuration.GetValue<string>("Deskpro:BaseAddress")),
            AuthorizationKey = Guard.Against.NullOrEmpty(configuration.GetValue<string>("Deskpro:AuthorizationKey"))
        };

        services.AddDeskpro(deskproOptions);

        mediatRAssemblies.Add(typeof(ModuleServices).Assembly);

        return services;
    }
}
