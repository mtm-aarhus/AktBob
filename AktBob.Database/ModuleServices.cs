using AktBob.Database.UseCases.Cases;
using AktBob.Database.UseCases.Messages;
using AktBob.Database.UseCases.Tickets;
using Ardalis.GuardClauses;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AktBob.Database;
public static class ModuleServices
{
    public static IServiceCollection AddDatabaseModule(this IServiceCollection services, IConfiguration configuration, List<Type> mediatorHandlers)
    {
        Guard.Against.NullOrEmpty(configuration.GetConnectionString("Database"));

        services.AddTransient<ISqlDataAccess, SqlDataAccess>();

        mediatorHandlers.AddRange([
            typeof(GetCaseByIdQueryHandler),
            typeof(GetCasesByTicketIdQueryHandler),
            typeof(AddCaseCommandHandler),
            typeof(GetCasesQueryHandler),
            typeof(UpdateCaseCommandHandler),
            typeof(GetMessageByIdQueryHandler),
            typeof(DeleteMessageCommandHandler),
            typeof(GetMessageByDeskproMessageIdQueryHandler),
            typeof(UpdateMessageSetGoDocumentIdCommandHandler),
            typeof(AddTicketCommandHandler),
            typeof(GetTicketByIdQueryHandler),
            typeof(GetTicketsQueryHandler),
            typeof(UpdateTicketCommandHandler),
            typeof(AddMessagesCommandHandler)]);

        return services;
    }
}
