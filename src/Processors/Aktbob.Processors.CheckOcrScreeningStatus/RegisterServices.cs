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
        services.AddHostedService<EmailNotificationBackgroundJob>();
        services.AddHostedService<NotificationDispatcherBackgroundJob>();
        services.AddHostedService<PodioNotificationBackgroundJob>();
        services.AddHostedService<QueryFileBackgroundJob>();
        services.AddHostedService<UpdatePodioItemBackgroundJob>();
        services.AddHostedService<RegisterFilesBackgroundJob>();
        
        services.AddScoped<EmailNotificationHandler>();
        services.AddScoped<NotificationDispatcherHandler>();
        services.AddScoped<PodioNotificationHandler>();
        services.AddScoped<QueryFileHandler>();
        services.AddScoped<UpdatePodioItemHandler>();
        services.AddScoped<RegisterFilesHandler>();
        
        return services;
    }
}