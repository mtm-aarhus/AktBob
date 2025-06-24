using AktBob.Api;
using AktBob.Api.Endpoints;
using AktBob.Api.Endpoints.Cases;
using AktBob.Shared;
using FastEndpoints;
using Hangfire;
using Microsoft.AspNetCore.Authentication;
using AktBob.Database;
using AktBob.Shared.Middlewares;
using AktBob.Shared.ModuleClients;
using Hangfire.Dashboard.BasicAuthorization;
using Ardalis.GuardClauses;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Fast Endpoints
builder.Services.AddFastEndpoints(options =>
    options.Assemblies = [
        typeof(Program).Assembly,
        typeof(AktBob.Database.ModuleServices).Assembly]
    );

builder.Services
    .AddAuthorization()
    .AddAuthentication(ApiKeyAuthentication.SchemeName)
    .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthentication>(ApiKeyAuthentication.SchemeName, null);

builder.Services.AddAntiforgery();

// OpenAPI
builder.Services.AddOpenApi();

// Hangfire
builder.Services.AddSingleton<IJobDispatcher, HangfireJobDispatcher>();
builder.Services.AddHangfire(config => config.UseSqlServerStorage(builder.Configuration.GetConnectionString("Hangfire")));

// Modules
builder.Services.AddDatabaseModule(builder.Configuration);
// builder.Services.AddPodioModule(builder.Configuration);
builder.Services.AddSharedModule();

// Module clients
builder.Services.AddPodioModuleClient(builder.Configuration);   

// Transactions
builder.Services.AddScoped<CreateCaseTransaction>();

var app = builder.Build();

app.MapOpenApi();
app.MapScalarApiReference("/");

var options = new DashboardOptions
{
    Authorization =
    [
        new BasicAuthAuthorizationFilter(
            new BasicAuthAuthorizationFilterOptions
            {
                RequireSsl = false,
                SslRedirect = false,
                LoginCaseSensitive = true,
                Users =
                [
                    new BasicAuthAuthorizationUser
                    {
                        Login = Guard.Against.NullOrEmpty(app.Configuration.GetValue<string>("HangfireDashboard:Username")),
                        PasswordClear = Guard.Against.NullOrEmpty(app.Configuration.GetValue<string>("HangfireDashboard:Password"))
                    }
                ]
            }
        )
    ]
};

app.UseHangfireDashboard("/hangfire", options);

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery(); 

app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseMiddleware<GlobalRequestLoggingMiddleware>();

app.UseFastEndpoints(c =>
{
    c.Endpoints.RoutePrefix = "api";
    c.Endpoints.Configurator = ep =>
    {
        ep.Description(b => b.ClearDefaultProduces());
    };
});

app.MapRootEndpoints();
app.MapJobEndpoints();
app.MapTicketEndpoints();
app.MapCaseEndpoints();
app.MapSubmissionEndpoints();
app.MapCleanUpQueueEndpoints();
app.MapDatabaseEndpoints();
app.MapPodioEndpoints();

app.Run();