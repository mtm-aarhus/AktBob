using Aktbob.Processors.CheckOcrScreeningStatus.Features.EmailNotification;
using Aktbob.Processors.CheckOcrScreeningStatus.Features.NotificationDispatcher;
using Aktbob.Processors.CheckOcrScreeningStatus.Features.PodioNotification;
using Aktbob.Processors.CheckOcrScreeningStatus.Features.QueryFile;
using Aktbob.Processors.CheckOcrScreeningStatus.Features.RegisterFiles;
using Aktbob.Processors.CheckOcrScreeningStatus.Features.UpdatePodioItem;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Aktbob.Processors.CheckOcrScreeningStatus;

public static class RegisterServices
{
    public static IServiceCollection AddCheckOcrScreeningStatusProcessor(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHostedService<EmailNotificationBackgroundService>();
        services.AddHostedService<NotificationDispatcherBackgroundService>();
        services.AddHostedService<PodioNotificationBackgroundService>();
        services.AddHostedService<QueryFileBackgroundService>();
        services.AddHostedService<UpdatePodioItemBackgroundService>();
        services.AddHostedService<RegisterFilesBackgroundService>();
        
        services.AddScoped<EmailNotificationHandler>();
        services.AddScoped<NotificationDispatcherHandler>();
        services.AddScoped<PodioNotificationHandler>();
        services.AddScoped<QueryFileHandler>();
        services.AddScoped<UpdatePodioItemHandler>();
        services.AddScoped<RegisterFilesHandler>();
        
        return services;
    }
}