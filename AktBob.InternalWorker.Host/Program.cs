using AktBob.CheckOCRScreeningStatus;
using Serilog;
using AktBob.Email;
using AktBob.Queue;
using AktBob.UiPath;
using AktBob.Deskpro;
using AktBob.PodioHookProcessor;
using AktBob.Podio;
using AktBob.OpenOrchestrator;
using AktBob.CloudConvert;
using MassTransit;
using Hangfire;
using AktBob.JobHandlers;
using AktBob.GetOrganized;

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
        services.AddEmailModuleServices(hostContext.Configuration, mediatorHandlers);
        services.AddQueueModule(hostContext.Configuration, mediatorHandlers);
        services.AddUiPathModule(hostContext.Configuration, mediatorHandlers);
        services.AddPodioHookProcessorModule(hostContext.Configuration);
        services.AddDeskproModule(hostContext.Configuration, mediatorHandlers);
        services.AddPodioModule(hostContext.Configuration, mediatorHandlers);
        services.AddOpenOrchestratorModule(hostContext.Configuration, mediatorHandlers);
        services.AddCloudConvertModule(hostContext.Configuration, mediatorHandlers);
        services.AddGetOrganizedModule(hostContext.Configuration, mediatorHandlers);
        services.AddJobHandlers(hostContext.Configuration);

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