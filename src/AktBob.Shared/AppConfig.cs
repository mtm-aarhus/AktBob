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
}
