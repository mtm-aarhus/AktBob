namespace AktBob.Shared;

public interface IAppConfig
{
    string GetConnectionString(string key);
    string? GetSection(string key);
    T? GetValue<T>(string key);
}