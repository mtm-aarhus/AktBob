using AktBob.Database.Contracts;
using AktBob.Database.JobHandlers;
using AktBob.Database.Jobs;
using AktBob.Database.Repositories;
using AktBob.Shared;
using Ardalis.GuardClauses;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Data;

namespace AktBob.Database;
public static class ModuleServices
{
    public static IServiceCollection AddDatabaseModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = Guard.Against.NullOrEmpty(configuration.GetConnectionString("Database"));

        services.AddScoped<IDbConnection>(x => new SqlConnection(connectionString));
        services.AddScoped<ISqlDataAccess, SqlDataAccess>();

        // Repositories
        services.AddScoped<IMessageRepository, MessageRepository>();
        services.AddScoped<ITicketRepository, TicketRepository>();
        services.AddScoped<ICaseRepository, CaseRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        
        // Jobs
        services.AddScoped<IJobHandler<DeleteMessageJob>, DeleteMessage>();

        return services;
    }
}
