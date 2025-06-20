using Aktbob.Modules.Podio;
using Aktbob.Modules.Podio.Endpoints;
using AktBob.Shared;
using AktBob.Shared.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// Logging
builder.Services.ConfigureLogging();

// Modules
builder.Services.AddModuleServices(builder.Configuration);

var app = builder.Build();
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseMiddleware<GlobalRequestLoggingMiddleware>();

app.MapModuleEndpoints();

app.Run();