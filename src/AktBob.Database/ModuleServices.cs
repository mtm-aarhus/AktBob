using AktBob.Database.Contracts;
using AktBob.Database.Decorators;
using AktBob.Database.Repositories;
using AktBob.Shared.DataAccess;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AktBob.Database;
public static class ModuleServices
{
    public static IServiceCollection AddDatabaseModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IDatabaseSqlConnectionFactory, DatabaseSqlConnectionFactory>();
        services.AddScoped<ISqlDataAccess<IDatabaseSqlConnectionFactory>>(provider =>
        {
            var inner = new SqlDataAccess<IDatabaseSqlConnectionFactory>(provider.GetRequiredService<IDatabaseSqlConnectionFactory>());

            var withLogging = new SqlDataAccessLoggingDecorator<IDatabaseSqlConnectionFactory>(
                inner,
                provider.GetRequiredService<ILogger<SqlDataAccess<IDatabaseSqlConnectionFactory>>>());

            var withException = new SqlDataAccessExceptionDecorator<IDatabaseSqlConnectionFactory>(
                withLogging,
                provider.GetRequiredService<ILogger<SqlDataAccess<IDatabaseSqlConnectionFactory>>>());

            return withException;
        });

        // Repositories
        services.AddScoped<IMessageRepository>(provider =>
        {
            var inner = new MessageRepository(provider.GetRequiredService<ISqlDataAccess<IDatabaseSqlConnectionFactory>>());

            var withLogging = new MessageRepositoryLoggingDecorator(
                inner,
                provider.GetRequiredService<ILogger<MessageRepository>>());

            var withExceptionHandling = new MessageRepositoryExceptionDecorator(
                withLogging,
                provider.GetRequiredService<ILogger<MessageRepository>>());

            return withExceptionHandling;
        });

        services.AddScoped<ITicketRepository>(provider =>
        {
            var inner = new TicketRepository(provider.GetRequiredService<ISqlDataAccess<IDatabaseSqlConnectionFactory>>());
            
            var withLogging = new TicketRepositoryLoggingDecorator(
                inner,
                provider.GetRequiredService<ILogger<TicketRepository>>());

            var withExceptionHandling = new TicketRepositoryExceptionDecorator(
                withLogging,
                provider.GetRequiredService<ILogger<TicketRepository>>());

            return withExceptionHandling;
        });

        services.AddScoped<ICaseRepository>(provider =>
        {
            var inner = new CaseRepository(provider.GetRequiredService<ISqlDataAccess<IDatabaseSqlConnectionFactory>>());

            var withLogging = new CaseRepositoryLoggingDecorator(
                inner,
                provider.GetRequiredService<ILogger<CaseRepository>>());

            var withExceptionHandling = new CaseRepositoryExceptionDecorator(
                withLogging,
                provider.GetRequiredService<ILogger<CaseRepository>>());

            return withExceptionHandling;
        });

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        
        return services;
    }
}
