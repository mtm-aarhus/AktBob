using AAK.FilArkiv;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AktBob.FilArkiv.Handlers.GetDocumentsByCaseId;

internal static class RegisterHandler
{
    public static IServiceCollection AddGetDocumentsByCaseIdHandler(this IServiceCollection services)
    {
        services.AddScoped<IGetDocumentsByCaseIdHandler>(provider =>
        {
            var logger = provider.GetRequiredService<ILogger<GetDocumentsByCaseIdHandler>>();
            
            var inner = new GetDocumentsByCaseIdHandler(provider.GetRequiredService<IFilArkiv>());
            var withLogging = new GetDocumentsByCaseIdHandlerLogging(inner, logger);
            var withException = new GetDocumentsByCaseIdHandlerException(withLogging, logger);
            
            return withException;
        });
        
        return services;
    }
}