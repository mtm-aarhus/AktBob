using AktBob.Shared.Types.Deskpro;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;

namespace AktBob.Shared.Types;
public static class RegisterConverters
{
    public static IServiceCollection AddConverters(this IServiceCollection services)
    {
        //services.Configure<JsonOptions>(options =>
        //{
        //    options.SerializerOptions.Converters.Add(new TicketIdConverter());
        //});

        return services;
    }
}
