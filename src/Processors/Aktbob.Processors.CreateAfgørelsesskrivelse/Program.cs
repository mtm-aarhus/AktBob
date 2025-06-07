using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Aktbob.Modules.Deskpro;
using Aktbob.Modules.OpenOrchestrator;

var builder = FunctionsApplication.CreateBuilder(args);

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

builder.Services.AddDeskproModuleClient(builder.Configuration);
builder.Services.AddOpenOrchestratorModuleClient(builder.Configuration);

builder.Build().Run();