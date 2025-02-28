using AktBob.Api;
using AktBob.Shared;
using FastEndpoints;
using FastEndpoints.Swagger;
using Hangfire;
using Microsoft.AspNetCore.Authentication;
using NSwag;
using AktBob.Database;
using AktBob.Podio;
using Hangfire.Dashboard.BasicAuthorization;
using Ardalis.GuardClauses;
using System.Reflection;
using AktBob.Shared.CQRS;

var builder = WebApplication.CreateBuilder(args);

// Fast Endpoints
builder.Services.AddFastEndpoints(options =>
    options.Assemblies = [
        typeof(Program).Assembly,
        typeof(AktBob.Database.ModuleServices).Assembly,
        typeof(AktBob.Podio.ModuleServices).Assembly]
    );

builder.Services
    .AddAuthorization()
    .AddAuthentication(ApiKeyAuthentication.SchemeName)
    .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthentication>(ApiKeyAuthentication.SchemeName, null);

// Swagger
builder.Services.SwaggerDocument(o =>
{
    o.EnableJWTBearerAuth = false;
    o.DocumentSettings = s =>
    {
        s.AddAuth(ApiKeyAuthentication.SchemeName, new()
        {
            Name = ApiKeyAuthentication.HeaderName,
            In = OpenApiSecurityApiKeyLocation.Header,
            Type = OpenApiSecuritySchemeType.ApiKey
        });

        s.Title = "AktBob API";
    };

    o.AutoTagPathSegmentIndex = 0;
});

// Hangfire
builder.Services.AddSingleton<IJobDispatcher, HangfireJobDispatcher>();
builder.Services.AddHangfire(config => config.UseSqlServerStorage(builder.Configuration.GetConnectionString("Hangfire")));

// Modules
var cqrsHandlersAssemblies = new List<Assembly>();
builder.Services.AddDatabaseModule(builder.Configuration, cqrsHandlersAssemblies);
builder.Services.AddPodioModule(builder.Configuration, cqrsHandlersAssemblies);

// CQRS
builder.Services.AddSingleton<ICommandDispatcher, CommandDispatcher>();
builder.Services.AddSingleton<IQueryDispatcher, QueryDispatcher>();
builder.Services.AddCQRSHandlers(cqrsHandlersAssemblies.ToArray());

var app = builder.Build();

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

app.UseFastEndpoints(c =>
{
    c.Endpoints.RoutePrefix = "Api";
    c.Endpoints.Configurator = ep =>
    {
        ep.Description(b => b.ClearDefaultProduces());
    };
});

app.UseSwaggerGen();
app.Run();