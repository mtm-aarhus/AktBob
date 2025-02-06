using AktBob.CheckOCRScreeningStatus;
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
        services.AddCheckOCRScreeningStatusModule(hostContext.Configuration, mediatorHandlers);
        services.AddUiPathModule(hostContext.Configuration, mediatorHandlers);
        services.AddDeskproModule(hostContext.Configuration, mediatorHandlers);
        services.AddPodioModule(hostContext.Configuration, mediatorHandlers);
        services.AddOpenOrchestratorModule(hostContext.Configuration, mediatorHandlers);
        services.AddCloudConvertModule(hostContext.Configuration, mediatorHandlers);
        services.AddGetOrganizedModule(hostContext.Configuration, mediatorHandlers);
        services.AddJobHandlers(hostContext.Configuration);
        services.AddDatabaseModule(hostContext.Configuration, mediatorHandlers);

        // MassTransit Mediator
        services.AddMediator(cfg =>
        {
            cfg.AddConsumers(mediatorHandlers.ToArray());
        });

        // Hangfire
        services.AddHangfire(config => config.UseSqlServerStorage(hostContext.Configuration.GetConnectionString("Hangfire")));
        services.AddHangfireServer();
    });


var host = builder.Build();
host.Run();