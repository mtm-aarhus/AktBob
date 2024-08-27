using AktBob.CheckOCRScreeningStatus;
using System.Reflection;
using MediatR.NotificationPublishers;
using JNJ.MessageBus;
using Serilog;
using AktBob.Email;
using AktBob.Queue;
using AktBob.UiPath;
using AktBob.Deskpro;
using AktBob.PodioHookProcessor;

var builder = Host.CreateDefaultBuilder(args)
    .UseWindowsService()
    .ConfigureServices((hostContext, services) =>
    {
        // Serilog
        services.AddSerilog(config =>
        {
            config.ReadFrom.Configuration(hostContext.Configuration);
        });

        // Modules
        var mediatrAssemblies = new List<Assembly>();
        services.AddCheckOCRScreeningStatusModule(hostContext.Configuration, mediatrAssemblies);
        services.AddEmailModuleServices(hostContext.Configuration, mediatrAssemblies);
        services.AddQueueModule(hostContext.Configuration, mediatrAssemblies);
        services.AddUiPathModule(hostContext.Configuration, mediatrAssemblies);
        services.AddPodioHookProcessorModule(hostContext.Configuration);
        services.AddDeskproModule(hostContext.Configuration, mediatrAssemblies);

        // Mediatr
        services.AddMediatR(c =>
        {
            c.RegisterServicesFromAssemblies(mediatrAssemblies.ToArray());
            c.NotificationPublisher = new TaskWhenAllPublisher();
        });

        // EventBus
        services.AddEventBus();
    });


var host = builder.Build();
host.Run();
