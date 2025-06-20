using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace AktBob.Shared.Middlewares;

public class GlobalRequestLoggingMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        // Enable buffering so the request body can be read multiple times
        context.Request.EnableBuffering();

        var requestBody = string.Empty;

        if (context.Request is { ContentLength: > 0, Body.CanSeek: true })
        {
            context.Request.Body.Position = 0;

            using var reader = new StreamReader(
                context.Request.Body,
                encoding: Encoding.UTF8,
                detectEncodingFromByteOrderMarks: false,
                leaveOpen: true);
            
            requestBody = await reader.ReadToEndAsync();
            context.Request.Body.Position = 0; // Reset for downstream middleware
        }
        logger.LogInformation("Request: {method} {path}{query}  content-type: {contentType}  body: {body}",
            context.Request.Method,
            context.Request.Path,
            context.Request.QueryString,
            context.Request.ContentType, 
            requestBody);
        
        await next(context);
    }
}