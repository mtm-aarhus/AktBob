using AAK.Deskpro;
using Aktbob.Modules.Deskpro.Features.DownloadMessageAttachment;
using Aktbob.Modules.Deskpro.Features.GetCustomFieldSpecifications;
using Aktbob.Modules.Deskpro.Features.GetMessage;
using Aktbob.Modules.Deskpro.Features.GetMessageAttachments;
using Aktbob.Modules.Deskpro.Features.GetMessages;
using Aktbob.Modules.Deskpro.Features.GetPersonByEmail;
using Aktbob.Modules.Deskpro.Features.GetPersonById;
using Aktbob.Modules.Deskpro.Features.GetTeam;
using Aktbob.Modules.Deskpro.Features.GetTicket;
using Aktbob.Modules.Deskpro.Features.GetTicketsByFieldSearch;
using Aktbob.Modules.Deskpro.Features.InvokeWebhook;
using AktBob.Shared;
using Microsoft.Extensions.DependencyInjection;

namespace Aktbob.Modules.Deskpro.Features;

internal static class RegisterHandlers
{
    public static IServiceCollection AddDownloadMessageAttachmentHandler(this IServiceCollection services)
    {
        services.AddScoped<IDownloadMessageAttachmentHandler>(provider =>
        {
            var logger = provider.GetRequiredService<ILogger<DownloadMessageAttachmentHandler>>();

            var inner = new DownloadMessageAttachmentHandler(provider.GetRequiredService<IDeskproClient>());
            var withLogging = new DownloadMessageAttachmentHandlerLogging(inner, logger);
            var withExceptionHandling = new DownloadMessageAttachmentHandlerException(withLogging, logger);

            return withExceptionHandling;
        });

        return services;
    }
    
    
    public static IServiceCollection AddGetCustomFieldSpecificationsHandler(this IServiceCollection services)
    {
        services.AddScoped<IGetCustomFieldSpecificationsHandler>(provider =>
        {
            var logger = provider.GetRequiredService<ILogger<GetCustomFieldSpecificationsHandler>>();

            var inner = new GetCustomFieldSpecificationsHandler(provider.GetRequiredService<IDeskproClient>());
            var withCaching = new GetCustomFieldSpecificationsHandlerCaching(inner, provider.GetRequiredService<ICacheService>());
            var withLogging = new GetCustomFieldSpecificationsHandlerLogging(withCaching, logger);
            var withExceptionHandling = new GetCustomFieldSpecificationsHandlerException(withLogging, logger);

            return withExceptionHandling;
        });

        return services;
    }
    
    
    public static IServiceCollection AddGetMessageHandler(this IServiceCollection services)
    {
        services.AddScoped<IGetMessageHandler>(provider =>
        {
            var logger = provider.GetRequiredService<ILogger<GetMessageHandler>>();

            var inner = new GetMessageHandler(provider.GetRequiredService<IDeskproClient>());
            var withCaching = new GetMessageHandlerCaching(inner, provider.GetRequiredService<ICacheService>());
            var withLogging = new GetMessageHandlerLogging(withCaching, logger);
            var withExceptionHandling = new GetMessageHandlerException(withLogging, logger);

            return withExceptionHandling;
        });

        return services;
    }
    
    
    public static IServiceCollection AddGetMessageAttachmentsHandler(this IServiceCollection services)
    {
        services.AddScoped<IGetMessageAttachmentsHandler>(provider =>
        {
            var logger = provider.GetRequiredService<ILogger<GetMessageAttachmentsHandler>>();

            var inner = new GetMessageAttachmentsHandler(provider.GetRequiredService<IDeskproClient>());
            var withCaching = new GetMessageAttachmentsHandlerCaching(inner, provider.GetRequiredService<ICacheService>());
            var withLogging = new GetMessageAttachmentsHandlerLogging(withCaching, logger);
            var withExceptionHandling = new GetMessageAttachmentsHandlerException(withLogging, logger);

            return withExceptionHandling;
        });

        return services;
    }
    
    
    public static IServiceCollection AddGetMessagesHandler(this IServiceCollection services)
    {
        services.AddScoped<IGetMessagesHandler>(provider =>
        {
            var logger = provider.GetRequiredService<ILogger<GetMessagesHandler>>();

            var inner = new GetMessagesHandler(
                provider.GetRequiredService<IDeskproClient>(),
                provider.GetRequiredService<IGetPersonByIdHandler>());

            var withLogging = new GetMessagesHandlerLogging(inner, logger);
            var withExceptionHandling = new GetMessagesHandlerException(withLogging, logger);

            return withExceptionHandling;
        });

        return services;
    }
    
    
    public static IServiceCollection AddGetPersonByEmailHandler(this IServiceCollection services)
    {
        services.AddScoped<IGetPersonByEmailHandler>(provider =>
        {
            var logger = provider.GetRequiredService<ILogger<GetPersonByEmailHandler>>();

            var inner = new GetPersonByEmailHandler(
                provider.GetRequiredService<IDeskproClient>(),
                provider.GetRequiredService<IAppConfig>());

            var withCaching = new GetPersonByEmailHandlerCaching(inner, provider.GetRequiredService<ICacheService>());
            var withLogging = new GetPersonByEmailHandlerLogging(withCaching, logger);
            var withExceptionHandling = new GetPersonByEmailHandlerException(withLogging, logger);

            return withExceptionHandling;
        });

        return services;
    }
    
    
    public static IServiceCollection AddGetPersonByIdHandler(this IServiceCollection services)
    {
        services.AddScoped<IGetPersonByIdHandler>(provider =>
        {
            var logger = provider.GetRequiredService<ILogger<GetPersonByIdHandler>>();

            var inner = new GetPersonByIdHandler(provider.GetRequiredService<IDeskproClient>());
            var withCaching = new GetPersonByIdHandlerCaching(inner, provider.GetRequiredService<ICacheService>());
            var withLogging = new GetPersonByIdHandlerLogging(withCaching, logger);
            var withExceptionHandling = new GetPersonByIdHandlerException(withLogging, logger);

            return withExceptionHandling;
        });

        return services;
    }
    
    
    public static IServiceCollection AddGetTeamHandler(this IServiceCollection services)
    {
        services.AddScoped<IGetTeamHandler>(provider =>
        {
            var logger = provider.GetRequiredService<ILogger<GetTeamHandler>>();

            var inner = new GetTeamHandler(provider.GetRequiredService<IDeskproClient>());
            var withCaching = new GetTeamHandlerCaching(inner, provider.GetRequiredService<ICacheService>());
            var withLogging = new GetTeamHandlerLogging(withCaching, logger);
            var withExceptionHandling = new GetTeamHandlerException(withLogging, logger);

            return withExceptionHandling;
        });

        return services;
    }
    
    
    public static IServiceCollection AddGetTicketHandler(this IServiceCollection services)
    {
        services.AddScoped<IGetTicketHandler>(provider =>
        {
            var logger = provider.GetRequiredService<ILogger<GetTicketHandler>>();

            var inner = new GetTicketHandler(provider.GetRequiredService<IDeskproClient>());
            var withLogging = new GetTicketHandlerLogging(inner, logger);
            var withExceptionHandling = new GetTicketHandlerException(withLogging, logger);

            return withExceptionHandling;
        });

        return services;
    }
    
    
    public static IServiceCollection AddGetTicketsByFieldSearchHandler(this IServiceCollection services)
    {
        services.AddScoped<IGetTicketsByFieldSearchHandler>(provider =>
        {
            var logger = provider.GetRequiredService<ILogger<GetTicketsByFieldSearchHandler>>();

            var inner = new GetTicketsByFieldSearchHandler(provider.GetRequiredService<IDeskproClient>());
            var withLogging = new GetTicketsByFieldSearchHandlerLogging(inner, logger);
            var withExceptionHandling = new GetTicketsByFieldSearchHandlerException(withLogging, logger);

            return withExceptionHandling;
        });

        return services;
    }
    
    
    public static IServiceCollection AddInvokeWebhookHandler(this IServiceCollection services)
    {
        services.AddScoped<IInvokeWebhookHandler>(provider =>
        {
            var logger = provider.GetRequiredService<ILogger<InvokeWebhookHandler>>();

            var inner = new InvokeWebhookHandler(provider.GetRequiredService<IDeskproClient>());
            var withLogging = new InvokeWebhookHandlerLogging(inner, logger);
            var withExceptionHandling = new InvokeWebhookHandlerException(withLogging, logger);

            return withExceptionHandling;
        });

        return services;
    }
}