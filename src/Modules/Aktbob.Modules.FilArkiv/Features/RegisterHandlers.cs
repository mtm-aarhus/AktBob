using AAK.FilArkiv;
using Aktbob.Modules.FilArkiv.Features.GetDocumentsByCaseId;
using Aktbob.Modules.FilArkiv.Features.GetFileProcessStatus;

namespace Aktbob.Modules.FilArkiv.Features;

internal static class RegisterHandlers
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
    
    public static IServiceCollection AddGetFileProcessStatusHandler(this IServiceCollection services)
    {
        services.AddScoped<IGetFileProcessStatusHandler>(provider =>
        {
            var logger = provider.GetRequiredService<ILogger<GetFileProcessStatusHandler>>();

            var inner = new GetFileProcessStatusHandler(provider.GetRequiredService<IFilArkiv>());
            var withLogging = new GetFileProcessStatusHandlerLogging(inner, logger);
            var withException = new GetFileProcessStatusHandlerException(withLogging, logger);

            return withException;
        });
        
        return services;
    }
}