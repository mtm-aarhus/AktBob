using AktBob.CheckOCRScreeningStatus;
using System.Reflection;
using Serilog;
using AktBob.Email;
using AktBob.Queue;
using AktBob.UiPath;
using AktBob.Deskpro;
using AktBob.PodioHookProcessor;
using AktBob.DatabaseAPI;
using AktBob.Podio;
using AktBob.JournalizeDocuments;
using AktBob.OpenOrchestrator;
using AktBob.CloudConvert;
using MassTransit;

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
        var massTransitConsumers = new List<Type>();
        services.AddCheckOCRScreeningStatusModule(hostContext.Configuration, mediatorHandlers, massTransitConsumers);
        services.AddEmailModuleServices(hostContext.Configuration, mediatorHandlers);
        services.AddQueueModule(hostContext.Configuration, mediatorHandlers);
        services.AddUiPathModule(hostContext.Configuration, mediatorHandlers);
        services.AddPodioHookProcessorModule(hostContext.Configuration);
        services.AddDeskproModule(hostContext.Configuration, mediatorHandlers);
        services.AddDatabaseApiModule(hostContext.Configuration, mediatorHandlers);
        services.AddPodioModule(hostContext.Configuration, mediatorHandlers);
        services.AddJournalizeDocumentsModule(hostContext.Configuration);
        services.AddOpenOrchestratorModule(hostContext.Configuration, mediatorHandlers);
        services.AddCloudConvertModule(hostContext.Configuration, mediatorHandlers);

        // MassTransit Mediator
        services.AddMediator(cfg =>
        {
            cfg.AddConsumers(mediatorHandlers.ToArray());
        });

        // MassTransit
        services.AddMassTransit(cfg =>
        {
            cfg.SetKebabCaseEndpointNameFormatter();

            // Use in-memory transport
            cfg.UsingInMemory((context, config) =>
            {
                config.ConfigureEndpoints(context);
            });

            cfg.AddConsumers(massTransitConsumers.ToArray());
        });
    });


var host = builder.Build();
host.Run();
