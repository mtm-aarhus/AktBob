using Serilog;
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
                    restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Error,
                    options: new()
                    {
                        To = Guard.Against.NullOrEmpty(configuration.GetSection("EmailLogEvents:To").Get<IEnumerable<string>>()).ToList(),
                        From = Guard.Against.NullOrEmpty(configuration.GetValue<string>("EmailLogEvents:From")),
                        Host = Guard.Against.NullOrEmpty(configuration.GetValue<string>("EmailLogEvents:Host")),
                        Port = Guard.Against.Null(configuration.GetValue<int?>("EmailLogEvents:Port")),
                        Subject = new MessageTemplateTextFormatter("{Timestamp:HH:mm:ss.fff} AktBob log messages"),
                        ConnectionSecurity = MailKit.Security.SecureSocketOptions.None
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
        services.AddSingleton<FailedJobLoggingFilter>();
        services.AddHangfire(config =>
        {
            config.UseSqlServerStorage(hostContext.Configuration.GetConnectionString("Hangfire"));
            //config.UseFilter(new AutomaticRetryAttribute
            //{
            //    Attempts = 2,
            //    OnlyOn = [typeof(BusinessException)]
            //});
        });

        services.AddHangfireServer(config =>
        {
            config.WorkerCount = configuration.GetValue<int?>("Hangfire:Workers") ?? 20;
        });

        // Modules
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

// Register Hangfire filters
using var scope = host.Services.CreateScope();
GlobalJobFilters.Filters.Add(scope.ServiceProvider.GetRequiredService<FailedJobLoggingFilter>());

host.Run();