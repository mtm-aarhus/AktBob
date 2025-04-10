using Ardalis.GuardClauses;
using Microsoft.Extensions.Configuration;

namespace AktBob.Shared;
public class AppConfig : IAppConfig
{
    private readonly IConfiguration _configuration;

    public AppConfig(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public T? GetValue<T>(string key) => _configuration.GetValue<T>(key);

    public string? GetSection(string key) => _configuration.GetSection(key)?.Value;

    public IEnumerable<IConfigurationSection> GetSectionChildren(string key) => _configuration.GetSection(key).GetChildren();

    public string GetConnectionString(string key) => Guard.Against.NullOrEmpty(_configuration.GetConnectionString(key));
}
