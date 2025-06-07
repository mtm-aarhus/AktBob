using Aktbob.Modules.OpenOrchestrator;
using Aktbob.Modules.OpenOrchestrator.Endpoints;
using AktBob.Shared;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

var key = Environment.GetEnvironmentVariable("APPINSIGHTS_INSTRUMENTATIONKEY");
SerilogBootstrapper.ConfigureLogging(key);
builder.Services.AddSerilog();

builder.Services.AddModuleServices(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.

app.MapModuleEndpoints();
app.Run();
