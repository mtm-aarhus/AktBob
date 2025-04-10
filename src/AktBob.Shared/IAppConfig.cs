namespace AktBob.Shared;

public interface IAppConfig
{
    string GetConnectionString(string key);
    T? GetValue<T>(string key);
}