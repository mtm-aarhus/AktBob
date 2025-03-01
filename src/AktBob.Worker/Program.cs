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
        services.AddUiPathModule(hostContext.Configuration);
        services.AddDeskproModule(hostContext.Configuration);
        services.AddPodioModule(hostContext.Configuration);
        services.AddOpenOrchestratorModule(hostContext.Configuration);
        services.AddCloudConvertModule(hostContext.Configuration);
        services.AddGetOrganizedModule(hostContext.Configuration);
        services.AddDatabaseModule(hostContext.Configuration);
        services.AddJobHandlers(hostContext.Configuration);
        services.AddJobHandlersModule(hostContext.Configuration);
        services.AddEmailModuleServices(hostContext.Configuration);
        services.AddSharedModule();

        // Hangfire
        services.AddTransient<FailedJobNotificationFilter>();
        services.AddHangfire(config =>
        {
            config.UseSqlServerStorage(hostContext.Configuration.GetConnectionString("Hangfire"));
        });
        services.AddHangfireServer();
    });


var host = builder.Build();

// Setup filter for dispatching notifications when a Hangfire job fails
var failedJobNotificationFilter = host.Services.GetRequiredService<FailedJobNotificationFilter>();
GlobalJobFilters.Filters.Add(failedJobNotificationFilter);

host.Run();