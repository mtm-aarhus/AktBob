using Aktbob.Modules.Deskpro;
using Aktbob.Modules.Deskpro.Api.Endpoints;
using AktBob.Shared;
using AktBob.Shared.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// Logging
builder.Services.ConfigureLogging();

// Modules
builder.Services.AddSharedModule();

var deskproBaseAddress = builder.Configuration.GetValue<string>("BaseAddress") ?? string.Empty;
var deskproKey = builder.Configuration.GetValue<string>("AuthorizationKey") ?? string.Empty;
builder.Services.AddDeskproModule(deskproBaseAddress, deskproKey);

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseMiddleware<GlobalRequestLoggingMiddleware>();

app.MapModuleEndpoints();

app.Run();