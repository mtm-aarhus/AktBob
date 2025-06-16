using AktBob.Database;
using AktBob.Shared;
using AktBob.Shared.ModuleClients;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

// Logging
builder.Services.ConfigureLogging();

// Modules
builder.Services.AddDatabaseModule(builder.Configuration);
builder.Services.AddSharedModule();

// Module clients
builder.Services.AddOpenOrchestratorModuleClient(builder.Configuration);
builder.Services.AddPodioModuleClient(builder.Configuration);
builder.Services.AddDeskproModuleClient(builder.Configuration);

builder.Build().Run();