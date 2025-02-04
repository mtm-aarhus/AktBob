using AktBob.DatabaseAPI.UseCases;
using Ardalis.GuardClauses;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AktBob.DatabaseAPI;

public static class ModuleServices
{
    public static IServiceCollection AddDatabaseApiModule(this IServiceCollection services, IConfiguration configuration, List<Type> mediatorHandlers)
    {
        services.AddTransient<IDatabaseApi, DatabaseApi>();
        services.AddHttpClient<IDatabaseApi, DatabaseApi>(client =>
        {
            client.BaseAddress = new Uri(Guard.Against.NullOrEmpty(configuration.GetValue<string>("DatabaseApi:Url")));
            client.DefaultRequestHeaders.Add("ApiKey", Guard.Against.NullOrEmpty(configuration.GetValue<string>("DatabaseApi:ApiKey")));
        });

        mediatorHandlers.AddRange([
            typeof(DeleteMessageCommandHandler),
            typeof(GetMessageByDeskproMessageIdQueryHandler),
            typeof(GetMessagesNotJournalizedQueryHandler),
            typeof(GetTicketByDeskproIdQueryHandler),
            typeof(GetTicketByPodioItemIdQueryHandler),
            typeof(PostCaseCommandHandler),
            typeof(UpdateCaseSetFilArkivCaseIdCommandHandler),
            typeof(UpdateMessageSetGoDocumentIdCommandHandler)]);

        return services;
    }
}
