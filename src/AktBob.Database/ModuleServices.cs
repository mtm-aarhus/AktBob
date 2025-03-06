using AktBob.Database.Contracts;
using AktBob.Database.DataAccess;
using AktBob.Database.Repositories;
using Ardalis.GuardClauses;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Data;

namespace AktBob.Database;
public static class ModuleServices
{
    public static IServiceCollection AddDatabaseModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = Guard.Against.NullOrEmpty(configuration.GetConnectionString("Database"));

        services.AddScoped<IDbConnection>(x => new SqlConnection(connectionString));

        services.AddScoped<ISqlDataAccess>(provider =>
        {
            var inner = new SqlDataAccess(provider.GetRequiredService<IDbConnection>());

            var withLogging = new SqlDataAccessLoggingDecorator(
                inner,
                provider.GetRequiredService<ILogger<SqlDataAccessLoggingDecorator>>());

            var withException = new SqlDataAccessExceptionDecorator(
                withLogging,
                provider.GetRequiredService<ILogger<SqlDataAccessExceptionDecorator>>());

            return withException;
        });

        // Repositories
        services.AddScoped<IMessageRepository>(provider =>
        {
            var inner = new MessageRepository(provider.GetRequiredService<ISqlDataAccess>());

            var withLogging = new MessageRepositoryLoggingDecorator(
                inner,
                provider.GetRequiredService<ILogger<MessageRepositoryLoggingDecorator>>());

            var withExceptionHandling = new MessageRepositoryExceptionDecorator(
                withLogging,
                provider.GetRequiredService<ILogger<MessageRepositoryExceptionDecorator>>());

            return withExceptionHandling;
        });

        services.AddScoped<ITicketRepository>(provider =>
        {
            var inner = new TicketRepository(provider.GetRequiredService<ISqlDataAccess>());
            
            var withLogging = new TicketRepositoryLoggingDecorator(
                inner,
                provider.GetRequiredService<ILogger<TicketRepositoryLoggingDecorator>>());

            var withExceptionHandling = new TicketRepositoryExceptionDecorator(
                withLogging,
                provider.GetRequiredService<ILogger<TicketRepositoryExceptionDecorator>>());

            return withExceptionHandling;
        });

        services.AddScoped<ICaseRepository>(provider =>
        {
            var inner = new CaseRepository(provider.GetRequiredService<ISqlDataAccess>());

            var withLogging = new CaseRepositoryLoggingDecorator(
                inner,
                provider.GetRequiredService<ILogger<CaseRepositoryLoggingDecorator>>());

            var withExceptionHandling = new CaseRepositoryExceptionDecorator(
                withLogging,
                provider.GetRequiredService<ILogger<CaseRepositoryExceptionDecorator>>());

            return withExceptionHandling;
        });

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        
        return services;
    }
}
