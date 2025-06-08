using AktBob.Database;
using AktBob.Shared;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using AktBob.Shared.Contracts.Modules.Deskpro;
using AktBob.Shared.Contracts.Modules.OpenOrchestrator;
using AktBob.Shared.ModuleClients;
using Microsoft.Extensions.Azure;

var builder = FunctionsApplication.CreateBuilder(args);

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

builder.Services.AddSharedModule();
builder.Services.AddDatabaseModule(builder.Configuration);
builder.Services.AddDeskproModuleClient(builder.Configuration);
builder.Services.AddOpenOrchestratorModuleClient(builder.Configuration);

builder.Build().Run();