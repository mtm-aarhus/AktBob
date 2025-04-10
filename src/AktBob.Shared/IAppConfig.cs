namespace AktBob.Shared;

public interface IAppConfig
{
    T? GetValue<T>(string key);
}