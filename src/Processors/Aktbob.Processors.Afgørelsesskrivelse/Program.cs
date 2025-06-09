using AktBob.Database;
using AktBob.Shared;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using AktBob.Shared.ModuleClients;
using Serilog;

var builder = FunctionsApplication.CreateBuilder(args);

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

// Logging
builder.Services.ConfigureLogging();

// Modules
builder.Services.AddSharedModule();
builder.Services.AddDatabaseModule(builder.Configuration);

// Module clients
builder.Services.AddDeskproModuleClient(builder.Configuration);
builder.Services.AddOpenOrchestratorModuleClient(builder.Configuration);

builder.Build().Run();