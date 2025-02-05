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
            typeof(UseCases.Cases.AddCase.AddCaseCommandHandler),
            typeof(UseCases.Cases.GetCaseById.GetCaseByIdQueryHandler),
            typeof(UseCases.Cases.GetCases.GetCasesQueryHandler),
            typeof(UseCases.Cases.GetCasesByTicketId.GetCasesByTicketIdQueryHandler),
            typeof(UseCases.Cases.PatchCase.PatchCaseCommandHandler),
            typeof(UseCases.Messages.ClearQueuedForJournalization.ClearQueuedForJournalizationCommandHandler),
            typeof(UseCases.Messages.DeleteMessage.DeleteMessageCommandHandler),
            typeof(UseCases.Messages.GetMessageByDeskproMessageId.GetMessageByDeskproMessageIdQueryHandler),
            typeof(UseCases.Messages.GetMessageById.GetMessageByIdQueryHandler),
            typeof(UseCases.Messages.GetMessages.GetMessagesQueryHandler),
            typeof(UseCases.Messages.PatchMessage.PatchMessageCommandHandler),
            typeof(UseCases.Messages.PostMessage.PostMessageCommandHandler),
            typeof(UseCases.Tickets.AddTicket.AddTicketCommandHandler),
            typeof(UseCases.Tickets.GetTicketById.GetTicketByIdQueryHandler),
            typeof(UseCases.Tickets.GetTickets.GetTicketsQueryHandler),
            typeof(UseCases.Tickets.PatchTicket.PatchTicketCommandHandler)]);

        return services;
    }
}
