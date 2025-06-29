using AAK.Podio;
using Aktbob.Modules.Podio.Features.GetItem;
using Aktbob.Modules.Podio.Features.PostComment;
using Aktbob.Modules.Podio.Features.UpdateTextField;

namespace Aktbob.Modules.Podio.Features;

internal static class RegisterHandlers
{
    public static IServiceCollection AddGetItemHandler(this IServiceCollection services)
    {
        services.AddScoped<IGetItemHandler>(provider =>
        {
            var logger = provider.GetRequiredService<ILogger<GetItemHandler>>();

            var inner = new GetItemHandler(
                provider.GetRequiredService<IPodioFactory>(),
                provider.GetRequiredService<IConfigurationHelper>());

            var withLogging = new GetItemHandlerLogging(inner, logger);
            var withException = new GetItemHandlerException(withLogging, logger);

            return withException;
        });
        
        return services;
    }
    
    public static IServiceCollection AddPostCommentHandler(this IServiceCollection services)
    {
        services.AddScoped<IPostCommentHandler>(provider =>
        {
            var logger = provider.GetRequiredService<ILogger<PostCommentHandler>>();
            
            var inner = new PostCommentHandler(
                provider.GetRequiredService<IPodioFactory>(),
                provider.GetRequiredService<IConfigurationHelper>());

            var withLogging = new PostCommentHandlerLogging(inner, logger);
            var withException = new PostCommentHandlerException(withLogging, logger);
            
            return withException;
        });
        
        return services;
    }
    
    public static IServiceCollection AddUpdateTextFieldHandler(this IServiceCollection services)
    {
        services.AddScoped<IUpdateTextFieldHandler>(provider =>
        {
            var logger = provider.GetRequiredService<ILogger<UpdateTextFieldHandler>>();
            
            var inner = new UpdateTextFieldHandler(
                provider.GetRequiredService<IPodioFactory>(),
                provider.GetRequiredService<IConfigurationHelper>());

            var withLogging = new UpdateTextFieldHandlerLogging(inner, logger);
            var withException = new UpdateTextFieldHandlerException(withLogging, logger);
            
            return withException;
        });
        
        return services;
    }
}