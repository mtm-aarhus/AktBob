using Microsoft.Extensions.Configuration;

namespace AktBob.Shared;

public interface IAppConfig
{
    string GetConnectionString(string key);
    string? GetSection(string key);
    IEnumerable<IConfigurationSection> GetSectionChildren(string key);
    T? GetValue<T>(string key);
}