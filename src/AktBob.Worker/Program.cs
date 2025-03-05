using Serilog;
using AktBob.UiPath;
using AktBob.Deskpro;
using AktBob.Podio;
using AktBob.OpenOrchestrator;
using AktBob.CloudConvert;
using Hangfire;
using AktBob.Workflows;
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
            config.Enrich.FromLogContext();
            config.ReadFrom.Configuration(hostContext.Configuration);
        });

        // Modules
        services.AddUiPathModule(hostContext.Configuration);
        services.AddDeskproModule(hostContext.Configuration);
        services.AddPodioModule(hostContext.Configuration);
        services.AddOpenOrchestratorModule(hostContext.Configuration);
        services.AddCloudConvertModule(hostContext.Configuration);
        services.AddGetOrganizedModule(hostContext.Configuration);
        services.AddDatabaseModule(hostContext.Configuration);
        services.AddWorkflowJobs(hostContext.Configuration);
        services.AddWorkflowModule(hostContext.Configuration);
        services.AddEmailModuleServices(hostContext.Configuration);
        services.AddSharedModule();

        // Hangfire
        services.AddSingleton<IJobDispatcher, HangfireJobDispatcher>();
        services.AddScoped<FailedJobNotificationFilter>();
        services.AddHangfire(config =>
        {
            config.UseSqlServerStorage(hostContext.Configuration.GetConnectionString("Hangfire"));
        });
        services.AddHangfireServer();
    });


var host = builder.Build();

// Setup filter for dispatching notifications when a Hangfire job fails
using var scope = host.Services.CreateScope();
var failedJobNotificationFilter = scope.ServiceProvider.GetRequiredService<FailedJobNotificationFilter>();
GlobalJobFilters.Filters.Add(failedJobNotificationFilter);

host.Run();