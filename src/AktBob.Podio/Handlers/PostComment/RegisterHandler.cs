using AAK.Podio;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AktBob.Podio.Handlers.PostComment;

internal static class RegisterHandler
{
    public static IServiceCollection AddPostCommentHandler(this IServiceCollection services)
    {
        services.AddScoped<IPostCommentHandler>(provider =>
        {
            var logger = provider.GetRequiredService<ILogger<PostCommentHandler>>();
            
            var inner = new PostCommentHandler(
                provider.GetRequiredService<IPodioFactory>(),
                provider.GetRequiredService<IConfiguration>());

            var withLogging = new PostCommentHandlerLogging(inner, logger);
            var withException = new PostCommentHandlerException(withLogging, logger);
            
            return withException;
        });
        
        return services;
    }
}