using AktBob.Email;
using AktBob.Shared;

var builder = Host.CreateDefaultBuilder(args)
    .UseWindowsService()
    .ConfigureServices((hostContext, services) =>
    {
        var configuration = hostContext.Configuration;
        
        services.AddModuleServices(configuration);
        services.ConfigureLogging(configuration, hostContext.HostingEnvironment);
        services.AddSharedModule();
    });

var host = builder.Build();
host.Run();