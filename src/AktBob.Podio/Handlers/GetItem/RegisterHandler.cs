using AAK.Podio;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AktBob.Podio.Handlers.GetItem;

internal static class RegisterHandler
{
    public static IServiceCollection AddGetItemHandler(this IServiceCollection services)
    {
        services.AddScoped<IGetItemHandler>(provider =>
        {
            var logger = provider.GetRequiredService<ILogger<GetItemHandler>>();
            
            var inner = new GetItemHandler(
                provider.GetRequiredService<IPodioFactory>(),
                provider.GetRequiredService<IConfiguration>());

            var withLogging = new GetItemHandlerLogging(inner, logger);
            var withException = new GetItemHandlerException(withLogging, logger);
            
            return withException;
        });
        
        return services;
    }
}