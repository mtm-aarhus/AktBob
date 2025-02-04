using AAK.Deskpro;
using Ardalis.GuardClauses;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AktBob.Deskpro;

public static class ModuleServices
{
    public static IServiceCollection AddDeskproModule(this IServiceCollection services, IConfiguration configuration, List<Type> mediatorHandlers)
    {
        var deskproOptions = new DeskproOptions
        {
            BaseAddress = Guard.Against.NullOrEmpty(configuration.GetValue<string>("Deskpro:BaseAddress")),
            AuthorizationKey = Guard.Against.NullOrEmpty(configuration.GetValue<string>("Deskpro:AuthorizationKey"))
        };

        services.AddDeskpro(deskproOptions);

        mediatorHandlers.AddRange([
            typeof(GetDeskproCustomFieldSpecificationsQueryHandler),
            typeof(GetDeskproMessageAttachmentQueryHandler),
            typeof(GetDeskproMessageAttachmentsQueryHandler),
            typeof(GetDeskproMessageByIdQueryHandler),
            typeof(GetDeskproMessagesQueryHandler),
            typeof(GetDeskproPersonQueryHandler),
            typeof(GetDeskproTicketByIdQueryHandler),
            typeof(GetDeskproTicketsByFieldSearchQueryHandler)]);

        return services;
    }
}
