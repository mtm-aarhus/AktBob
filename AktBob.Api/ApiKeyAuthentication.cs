using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Security.Principal;
using System.Text.Encodings.Web;

namespace AktBob.Api;

internal sealed class ApiKeyAuthentication(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, IConfiguration configuration) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    internal const string SchemeName = "ApiKey";
    internal const string HeaderName = "ApiKey";

    readonly string _apiKey = configuration["ApiKey"] ?? throw new InvalidOperationException("Api key not set in appsettings.json");

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        Request.Headers.TryGetValue(HeaderName, out var extractedApiKey);

        if (!IsPublicEndpoint() && !extractedApiKey.Equals(_apiKey))
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid API credentials"));
        }

        var identity = new ClaimsIdentity(
            claims: new[] { new Claim("ClientID", "Default") },
            authenticationType: Scheme.Name);
        var principal = new GenericPrincipal(identity, roles: null);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }

    bool IsPublicEndpoint() => Context.GetEndpoint()?.Metadata.OfType<AllowAnonymousAttribute>().Any() is null or true;
}
