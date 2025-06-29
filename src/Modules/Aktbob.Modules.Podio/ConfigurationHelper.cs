namespace Aktbob.Modules.Podio;

public interface IConfigurationHelper
{
    string ClientId { get; }
    string GetClientSecret { get; }
    string GetAppToken(int appId);
}

internal class ConfigurationHelper(Dictionary<int, string> appTokens, string clientId, string clientSecret) : IConfigurationHelper
{
    public string ClientId { get; } = clientId;
    public string GetClientSecret { get; } = clientSecret;

    public string GetAppToken(int appId) => appTokens.TryGetValue(appId, out var value) ? value : string.Empty;
}