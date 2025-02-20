using Serilog;
using AktBob.UiPath;
using AktBob.Deskpro;
using AktBob.Podio;
using AktBob.OpenOrchestrator;
using AktBob.CloudConvert;
using MassTransit;
using Hangfire;
using AktBob.JobHandlers;
using AktBob.GetOrganized;
using AktBob.Database;
using AktBob.Worker;
using AktBob.Email;

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
        var mediatorHandlers = new List<Type>();
        services.AddUiPathModule(hostContext.Configuration, mediatorHandlers);
        services.AddDeskproModule(hostContext.Configuration, mediatorHandlers);
        services.AddPodioModule(hostContext.Configuration, mediatorHandlers);
        services.AddOpenOrchestratorModule(hostContext.Configuration, mediatorHandlers);
        services.AddCloudConvertModule(hostContext.Configuration, mediatorHandlers);
        services.AddGetOrganizedModule(hostContext.Configuration, mediatorHandlers);
        services.AddDatabaseModule(hostContext.Configuration, mediatorHandlers);
        services.AddJobHandlers(hostContext.Configuration);
        services.AddJobHandlersModule(hostContext.Configuration);
        services.AddEmailModuleServices(hostContext.Configuration, mediatorHandlers);

        // MassTransit Mediator
        services.AddMediator(cfg =>
        {
            cfg.AddConsumers(mediatorHandlers.ToArray());
        });

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