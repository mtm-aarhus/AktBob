using Aktbob.Modules.Podio;
using Aktbob.Modules.Podio.Api.Endpoints;
using AktBob.Shared;
using AktBob.Shared.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// Logging
builder.Services.ConfigureLogging();

// Modules
var baseAddress = builder.Configuration.GetValue<string>("BaseAddress") ?? string.Empty;
var clientId = builder.Configuration.GetValue<string>("ClientId") ?? string.Empty;
var clientSecret = builder.Configuration.GetValue<string>("ClientSecret") ?? string.Empty;
var appTokens = builder.Configuration.GetSection("AppTokens").Get<Dictionary<int, string>>() ?? new Dictionary<int, string>(); 
builder.Services.AddPodioModule(baseAddress, appTokens, clientId, clientSecret);

var app = builder.Build();
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseMiddleware<GlobalRequestLoggingMiddleware>();

app.MapModuleEndpoints();

app.Run();

