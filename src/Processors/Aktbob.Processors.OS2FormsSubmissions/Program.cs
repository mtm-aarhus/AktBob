using AktBob.Database;
using Aktbob.Processors.OS2FormsSubmissions.OS2FormsModule;
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
builder.Services.AddOS2FormsModule(builder.Configuration);
builder.Services.AddDeskproModuleClient(builder.Configuration);
builder.Services.AddDatabaseModule(builder.Configuration);
builder.Services.AddSharedModule();

builder.Build().Run();