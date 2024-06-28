using AktBob.CheckOCRScreeningStatus;
using System.Reflection;
using MediatR.NotificationPublishers;
using JNJ.MessageBus;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

// Serilog
builder.Services.AddSerilog(config =>
{
    config.ReadFrom.Configuration(builder.Configuration);
});

// Modules
var mediatrAssemblies = new List<Assembly>();
builder.Services.AddCheckOCRScreeningStatusModule(builder.Configuration, mediatrAssemblies);

// Mediatr
builder.Services.AddMediatR(c =>
{
    c.RegisterServicesFromAssemblies(mediatrAssemblies.ToArray());
    c.NotificationPublisher = new TaskWhenAllPublisher();
});

// EventBus
builder.Services.AddEventBus();


var host = builder.Build();
host.Run();
