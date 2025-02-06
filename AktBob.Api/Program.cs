using AktBob.Api;
using AktBob.Shared;
using FastEndpoints;
using FastEndpoints.Swagger;
using Hangfire;
using Microsoft.AspNetCore.Authentication;
using NSwag;
using AktBob.Database;
using MassTransit;
using Microsoft.AspNetCore.Mvc.Filters;
using AktBob.Podio;

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
var mediatorHandlers = new List<Type>();
builder.Services.AddDatabaseModule(builder.Configuration, mediatorHandlers);
builder.Services.AddPodioModule(builder.Configuration, mediatorHandlers);

// MassTransit Mediator
builder.Services.AddMediator(cfg =>
{
    cfg.AddConsumers(mediatorHandlers.ToArray());
});


var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new HangfireDashboardAuthorizationFilter() }
});


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