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
using Ardalis.GuardClauses;
using Serilog.Formatting.Display;

var builder = Host.CreateDefaultBuilder(args)
    .UseWindowsService()
    .ConfigureServices((hostContext, services) =>
    {
        var configuration = hostContext.Configuration;

        // Serilog
        services.AddSerilog(config =>
        {
            config.Enrich.FromLogContext();

            if (hostContext.HostingEnvironment.IsDevelopment())
            {
                config.WriteTo.Console(outputTemplate: "{Timestamp:HH:mm:ss.fff} [{Level:u3}] [{SourceContext}] {Message:j} {NewLine}{Exception}");
            }
            
            if (hostContext.HostingEnvironment.IsProduction())
            {
                config.WriteTo.File(
                    outputTemplate: "{Timestamp:HH:mm:ss.fff} [{Level:u3}] [{SourceContext}] {Message:j} {NewLine}{Exception}",
                    rollingInterval: RollingInterval.Day,
                    shared: true,
                    path: Guard.Against.NullOrEmpty(configuration.GetValue<string>("LogFilesPath")));
            }

            if (hostContext.Configuration.GetValue<bool?>("EmailLogEvents:Enabled") ?? false)
            {
                config.WriteTo.Email(
                    options: new()
                    {
                        To = Guard.Against.NullOrEmpty(configuration.GetSection("EmailLogEvents:To").Get<IEnumerable<string>>()).ToList(),
                        From = Guard.Against.NullOrEmpty(configuration.GetValue<string>("EmailLogEvents:From")),
                        Host = Guard.Against.NullOrEmpty(configuration.GetValue<string>("EmailLogEvents:Host")),
                        Port = Guard.Against.Null(configuration.GetValue<int?>("EmailLogEvents:Port")),
                        Subject = new MessageTemplateTextFormatter("{Timestamp:HH:mm:ss.fff} AktBob log messages")
                    },
                    batchingOptions: new()
                    {
                        BufferingTimeLimit = TimeSpan.FromMinutes(Guard.Against.Null(configuration.GetValue<int?>("EmailLogEvents:TimeLimitMinutes"))),
                        EagerlyEmitFirstEvent = false
                    });
            }

            config.ReadFrom.Configuration(hostContext.Configuration);
        });

        // Hangfire
        services.AddSingleton<IJobDispatcher, HangfireJobDispatcher>();
        services.AddScoped<FailedJobNotificationFilter>();
        services.AddHangfire(config =>
        {
            config.UseSqlServerStorage(hostContext.Configuration.GetConnectionString("Hangfire"));
        });

        services.AddHangfireServer(config =>
        {
            config.Queues = ["default"];
            config.WorkerCount = configuration.GetValue<int?>("Hangfire:DefaultWorkerCounter") ?? 5;

        });

        services.AddHangfireServer(config =>
        {
            config.Queues = ["check-ocr-screening-status"];
            config.WorkerCount = configuration.GetValue<int?>("Hangfire:CheckOCRScreeningStatusQueueWorkerCount") ?? 20;
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
    });


var host = builder.Build();

// Setup filter for dispatching notifications when a Hangfire job fails
using var scope = host.Services.CreateScope();
var failedJobNotificationFilter = scope.ServiceProvider.GetRequiredService<FailedJobNotificationFilter>();
GlobalJobFilters.Filters.Add(failedJobNotificationFilter);

host.Run();