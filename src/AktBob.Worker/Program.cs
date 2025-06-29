using Serilog;
using AktBob.Deskpro;
using AktBob.CloudConvert;
using Hangfire;
using AktBob.Workflows;
using AktBob.GetOrganized;
using AktBob.Database;
using Aktbob.Modules.Deskpro;
using Aktbob.Modules.FilArkiv;
using Aktbob.Modules.Podio;
using Aktbob.Processors.CheckOcrScreeningStatus;
using Aktbob.Processors.SendEmail;
using AktBob.Worker;
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
            config.WriteTo.Console(outputTemplate: "{Timestamp:HH:mm:ss.fff} [{Level:u3}] [{SourceContext}] {Message:j} {NewLine}{Exception}");
            
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
            // config.UseFilter(new AutomaticRetryAttribute
            // {
            //     Attempts = 2,
            //     OnlyOn = [typeof(BusinessException)]
            // });
        });

        services.AddHangfireServer(config =>
        {
            config.WorkerCount = configuration.GetValue<int?>("Hangfire:Workers") ?? 20;
        });

        // Modules
        services.AddCloudConvertModule(hostContext.Configuration);
        services.AddGetOrganizedModule(hostContext.Configuration);
        services.AddDatabaseModule(hostContext.Configuration);
        services.AddWorkflowJobs(hostContext.Configuration);
        services.AddSharedModule();

        AddFilArkivModule(configuration, services);
        AddPodioModule(configuration, services);
        AddDeskproModule(configuration, services);

        // Processors
        services.AddCheckOcrScreeningStatusProcessor(configuration);
        AddSendEmailProcessor(configuration, services);
    });


var host = builder.Build();

// Register Hangfire filters
using var scope = host.Services.CreateScope();
GlobalJobFilters.Filters.Add(scope.ServiceProvider.GetRequiredService<FailedJobLoggingFilter>());

host.Run();
return;


static void AddFilArkivModule(IConfiguration configuration, IServiceCollection services)
{
    var baseAddress = configuration.GetValue<string>("FilArkiv:BaseAddress") ?? string.Empty;
    var clientId = configuration.GetValue<string>("FilArkiv:ClientId") ?? string.Empty;
    var clientSecret = configuration.GetValue<string>("FilArkiv:ClientSecret") ?? string.Empty;
    var tokenEndpoint = configuration.GetValue<string>("FilArkiv:TokenEndpoint") ?? string.Empty;
    services.AddFilArkivModule(baseAddress, clientId, clientSecret,  tokenEndpoint);
}

void AddPodioModule(IConfiguration configuration, IServiceCollection services)
{
    var baseAddress = configuration.GetValue<string>("Podio:BaseAddress") ?? string.Empty;
    var clientId = configuration.GetValue<string>("Podio:ClientId") ?? string.Empty;
    var clientSecret = configuration.GetValue<string>("Podio:ClientSecret") ?? string.Empty;
    var appTokens = configuration.GetSection("Podio:AppTokens").Get<Dictionary<int, string>>() ?? new Dictionary<int, string>(); 
    services.AddPodioModule(baseAddress, appTokens, clientId, clientSecret);
}

void AddDeskproModule(IConfiguration configuration, IServiceCollection services)
{
    var baseAddress = configuration.GetValue<string>("Deskpro:BaseAddress") ?? string.Empty;
    var key = configuration.GetValue<string>("Deskpro:AuthorizationKey") ?? string.Empty;
    services.AddDeskproModule(baseAddress, key);
}

void AddSendEmailProcessor(IConfiguration configuration, IServiceCollection services)
{
    var from = Guard.Against.NullOrEmpty(configuration.GetValue<string>("EmailModule:From"));
    var smtp = Guard.Against.NullOrEmpty(configuration.GetValue<string>("EmailModule:SmtpUrl"));
    var port = Guard.Against.Null(configuration.GetValue<int>("EmailModule:Port"));
    services.AddSendEmailProcessor(from, smtp, port);
}