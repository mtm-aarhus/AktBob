using Ardalis.GuardClauses;
using Microsoft.Extensions.Configuration;

namespace AktBob.Podio;
internal static class ConfigurationHelper
{
    public static string GetClientId(IConfiguration configuration) => Guard.Against.NullOrEmpty(configuration.GetValue<string>("Podio:ClientId"));

    public static string GetClientSecret(IConfiguration configuration) => Guard.Against.NullOrEmpty(configuration.GetValue<string>("Podio:ClientSecret"));

    public static string GetAppToken(IConfiguration configuration, int appId)
    {
        var podioAppTokens = Guard.Against.NullOrEmpty(configuration.GetSection("Podio:AppTokens").GetChildren().ToDictionary(x => x.Key, x => x.Value));
        var appTokens = podioAppTokens.Select(p => new KeyValuePair<string, string>(p.Key, p.Value ?? string.Empty)).ToDictionary().AsReadOnly();
        var appToken = appTokens.First(x => x.Key == appId.ToString()).Value;

        return appToken;
    }
}