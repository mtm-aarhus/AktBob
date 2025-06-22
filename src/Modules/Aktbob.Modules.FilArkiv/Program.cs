using Aktbob.Modules.FilArkiv;
using Aktbob.Modules.FilArkiv.Endpoints;
using AktBob.Shared;
using AktBob.Shared.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// Logging
builder.Services.ConfigureLogging();

builder.Services.AddModuleServices(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseMiddleware<GlobalRequestLoggingMiddleware>();

app.MapModuleEndpoints();

app.Run();