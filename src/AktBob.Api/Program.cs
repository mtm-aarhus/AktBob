using AktBob.Api;
using AktBob.Api.Endpoints.CreateAfgørelsesskrivelse;
using AktBob.Api.Endpoints.JournalizeEverything;
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
using Scalar.AspNetCore;
using OpenApiOperation = Microsoft.OpenApi.Models.OpenApiOperation;

var builder = WebApplication.CreateBuilder(args);

// Fast Endpoints
// builder.Services.AddFastEndpoints(options =>
//     options.Assemblies = [
//         typeof(Program).Assembly,
//         typeof(AktBob.Database.ModuleServices).Assembly,
//         typeof(AktBob.Podio.ModuleServices).Assembly]
//     );

builder.Services
    .AddAuthorization()
    .AddAuthentication(ApiKeyAuthentication.SchemeName)
    .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthentication>(ApiKeyAuthentication.SchemeName, null);

// OpenAPI
builder.Services.AddOpenApi();
// builder.Services.SwaggerDocument(o =>
// {
//     o.EnableJWTBearerAuth = false;
//     o.DocumentSettings = s =>
//     {
//         s.AddAuth(ApiKeyAuthentication.SchemeName, new()
//         {
//             Name = ApiKeyAuthentication.HeaderName,
//             In = OpenApiSecurityApiKeyLocation.Header,
//             Type = OpenApiSecuritySchemeType.ApiKey
//         });
//
//         s.Title = "AktBob API";
//     };
//
//     o.AutoTagPathSegmentIndex = 0;
// });

// Hangfire
builder.Services.AddSingleton<IJobDispatcher, HangfireJobDispatcher>();
builder.Services.AddHangfire(config => config.UseSqlServerStorage(builder.Configuration.GetConnectionString("Hangfire")));

// Modules
builder.Services.AddDatabaseModule(builder.Configuration);
builder.Services.AddPodioModule(builder.Configuration);
builder.Services.AddSharedModule();

var app = builder.Build();

app.MapOpenApi();
app.MapScalarApiReference();

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

// app.UseFastEndpoints(c =>
// {
//     c.Endpoints.RoutePrefix = "Api";
//     c.Endpoints.Configurator = ep =>
//     {
//         ep.Description(b => b.ClearDefaultProduces());
//     };
// });

var jobs = app.MapGroup("/api/jobs")
    .WithTags("Jobs")
    .RequireAuthorization();

jobs.MapPost("/journalize-everything", JournalizeEverything.Endpoint).WithSummary("Journalisér alt").WithDescription(JournalizeEverything.Description);
jobs.MapPost("/create-afgoerelsesskrivelse", CreateAfgørelsesskrivelse.Endpoint).WithSummary("Opret afgørelsesskrivelse").WithDescription(CreateAfgørelsesskrivelse.Description);

app.Run();