using Serilog;
using AktBob.UiPath;
using AktBob.Deskpro;
using AktBob.Podio;
using AktBob.OpenOrchestrator;
using AktBob.CloudConvert;
using Hangfire;
using AktBob.JobHandlers;
using AktBob.GetOrganized;
using AktBob.Database;
using AktBob.Worker;
using AktBob.Email;
using System.Reflection;
using AktBob.Shared.CQRS;
using Microsoft.Extensions.DependencyInjection.Extensions;
using AktBob.Shared;

var builder = Host.CreateDefaultBuilder(args)
    .UseWindowsService()
    .ConfigureServices((hostContext, services) =>
    {
        // Serilog
        services.AddSerilog(config =>
        {
            config.ReadFrom.Configuration(hostContext.Configuration);
            config.Enrich.FromLogContext();
        });

        // Modules
        var cqrsHandlersAssemblies = new List<Assembly>();
        services.AddUiPathModule(hostContext.Configuration, cqrsHandlersAssemblies);
        services.AddDeskproModule(hostContext.Configuration, cqrsHandlersAssemblies);
        services.AddPodioModule(hostContext.Configuration, cqrsHandlersAssemblies);
        services.AddOpenOrchestratorModule(hostContext.Configuration, cqrsHandlersAssemblies);
        services.AddCloudConvertModule(hostContext.Configuration, cqrsHandlersAssemblies);
        services.AddGetOrganizedModule(hostContext.Configuration, cqrsHandlersAssemblies);
        services.AddDatabaseModule(hostContext.Configuration, cqrsHandlersAssemblies);
        services.AddJobHandlers(hostContext.Configuration);
        services.AddJobHandlersModule(hostContext.Configuration);
        services.AddEmailModuleServices(hostContext.Configuration, cqrsHandlersAssemblies);
        services.AddSharedModule(cqrsHandlersAssemblies);

        // Hangfire
        services.AddTransient<FailedJobNotificationFilter>();
        services.AddHangfire(config =>
        {
            config.UseSqlServerStorage(hostContext.Configuration.GetConnectionString("Hangfire"));
        });
        services.AddHangfireServer();
    });


var host = builder.Build();

var failedJobNotificationFilter = host.Services.GetRequiredService<FailedJobNotificationFilter>();
GlobalJobFilters.Filters.Add(failedJobNotificationFilter);

host.Run();