using Ardalis.GuardClauses;

namespace Aktbob.Modules.Podio;
internal static class ConfigurationHelper
{
    public static string GetClientId(IConfiguration configuration) => Guard.Against.NullOrEmpty(configuration.GetValue<string>("ClientId"));

    public static string GetClientSecret(IConfiguration configuration) => Guard.Against.NullOrEmpty(configuration.GetValue<string>("ClientSecret"));

    public static string GetAppToken(IConfiguration configuration, int appId) => Guard.Against.NullOrEmpty(configuration.GetValue<string>($"AppTokens:{appId}"));
}