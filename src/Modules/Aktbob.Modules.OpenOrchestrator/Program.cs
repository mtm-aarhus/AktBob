using Aktbob.Modules.OpenOrchestrator;
using Aktbob.Modules.OpenOrchestrator.Endpoints;
using AktBob.Shared;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureLogging();
builder.Services.AddModuleServices(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.

app.MapModuleEndpoints();
app.Run();
