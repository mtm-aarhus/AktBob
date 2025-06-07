using Aktbob.Modules.Deskpro;
using Aktbob.Modules.Deskpro.Endpoints;
using AktBob.Shared;
using AktBob.Shared.Middlewares;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

var key = Environment.GetEnvironmentVariable("APPINSIGHTS_INSTRUMENTATIONKEY");
SerilogBootstrapper.ConfigureLogging(key);
builder.Services.AddSerilog();

builder.Services.AddSharedModule();
builder.Services.AddModuleServices(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseMiddleware<GlobalExceptionMiddleware>();
app.MapModuleEndpoints();

app.Run();