using AktBob.Database.UseCases.Cases;
using AktBob.Database.UseCases.Messages;
using AktBob.Database.UseCases.Tickets;
using AktBob.Database.UseCases.Messages.PostMessage;
using Ardalis.GuardClauses;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;

namespace AktBob.Database;
public static class ModuleServices
{
    public static IServiceCollection AddDatabaseModule(this IServiceCollection services, IConfiguration configuration, List<Type> mediatorTypes)
    {
        Guard.Against.NullOrEmpty(configuration.GetConnectionString("Database"));

        services.AddTransient<ISqlDataAccess, SqlDataAccess>();

        services.AddSingleton(serviceProvider =>
        {
            return new ConcurrentDictionary<Guid, DeskproTicketWithNewMessage>();
        });

        services.AddHostedService<PostMessageBackgroundJob>();

        mediatorTypes.AddRange([
            typeof(UseCases.Cases.GetCaseById.GetCaseByIdQueryHandler),
            typeof(UseCases.Cases.GetCasesByTicketId.GetCasesByTicketIdQueryHandler),
            typeof(AddCaseCommandHandler),
            typeof(GetCasesQueryHandler),
            typeof(UpdateCaseCommandHandler),
            typeof(UseCases.Messages.ClearQueuedForJournalization.ClearQueuedForJournalizationCommandHandler),
            typeof(UseCases.Messages.GetMessageById.GetMessageByIdQueryHandler),
            typeof(PostMessageCommandHandler),
            typeof(DeleteMessageCommandHandler),
            typeof(GetMessageByDeskproMessageIdQueryHandler),
            typeof(GetMessagesQueryHandler),
            typeof(UpdateMessageCommandHandler),
            typeof(UseCases.Tickets.AddTicket.AddTicketCommandHandler),
            typeof(UseCases.Tickets.GetTicketById.GetTicketByIdQueryHandler),
            typeof(GetTicketsQueryHandler),
            typeof(UpdateTicketCommandHandler)]);

        return services;
    }
}
