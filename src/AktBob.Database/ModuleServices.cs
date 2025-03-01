using AktBob.Database.Contracts;
using AktBob.Database.Repositories;
using Ardalis.GuardClauses;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AktBob.Database;
public static class ModuleServices
{
    public static IServiceCollection AddDatabaseModule(this IServiceCollection services, IConfiguration configuration)
    {
        Guard.Against.NullOrEmpty(configuration.GetConnectionString("Database"));

        services.AddTransient<ISqlDataAccess, SqlDataAccess>();

        // Repositories
        services.AddTransient<IMessageRepository, MessageRepository>();
        services.AddTransient<ITicketRepository, TicketRepository>();
        services.AddTransient<ICaseRepository, CaseRepository>();
        services.AddTransient<IUnitOfWork, UnitOfWork>();
        

        return services;
    }
}
