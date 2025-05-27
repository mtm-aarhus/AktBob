using AAK.Podio;
using AktBob.Podio.Handlers.PostComment;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AktBob.Podio.Handlers.UpdateTextField;

internal static class RegisterHandler
{
    public static IServiceCollection AddUpdateTextFieldHandler(this IServiceCollection services)
    {
        services.AddScoped<IUpdateTextFieldHandler>(provider =>
        {
            var logger = provider.GetRequiredService<ILogger<UpdateTextFieldHandler>>();
            
            var inner = new UpdateTextFieldHandler(
                provider.GetRequiredService<IPodioFactory>(),
                provider.GetRequiredService<IConfiguration>());

            var withLogging = new UpdateTextFieldHandlerLogging(inner, logger);
            var withException = new UpdateTextFieldHandlerException(withLogging, logger);
            
            return withException;
        });
        
        return services;
    }
}