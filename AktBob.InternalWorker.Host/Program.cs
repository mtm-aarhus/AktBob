using AktBob.CheckOCRScreeningStatus;
using System.Reflection;
using JNJ.MessageBus;
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
        var mediatorAssemblies = new List<Assembly>();
        services.AddCheckOCRScreeningStatusModule(hostContext.Configuration, mediatorAssemblies);
        services.AddEmailModuleServices(hostContext.Configuration, mediatorAssemblies);
        services.AddQueueModule(hostContext.Configuration, mediatorAssemblies);
        services.AddUiPathModule(hostContext.Configuration, mediatorAssemblies);
        services.AddPodioHookProcessorModule(hostContext.Configuration);
        services.AddDeskproModule(hostContext.Configuration, mediatorAssemblies);
        services.AddDatabaseApiModule(hostContext.Configuration, mediatorAssemblies);
        services.AddPodioModule(hostContext.Configuration, mediatorAssemblies);
        services.AddJournalizeDocumentsModule(hostContext.Configuration, mediatorAssemblies);
        services.AddOpenOrchestratorModule(hostContext.Configuration, mediatorAssemblies);
        services.AddCloudConvertModule(hostContext.Configuration, mediatorAssemblies);

        // MassTransit Mediator
        services.AddMediator(cfg =>
        {
            cfg.AddConsumers(mediatorAssemblies.ToArray());
        });

        // EventBus
        services.AddEventBus();
    });


var host = builder.Build();
host.Run();
