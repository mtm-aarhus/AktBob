using Ardalis.GuardClauses;
using FilArkivCore.Web.Client;
using Microsoft.Extensions.Configuration;

namespace AktBob.CheckOCRScreeningStatus;
internal class FilArkiv : IFilArkiv
{
    private readonly IConfiguration _configuration;

    public FilArkiv(FilArkivCoreClient filArkivCoreClient, IConfiguration configuration)
    {
        FilArkivCoreClient = filArkivCoreClient;
        _configuration = configuration;
    }

    public FilArkivCoreClient FilArkivCoreClient { get; }

    public async Task GetToken()
    {
        var clientId = Guard.Against.NullOrEmpty(_configuration.GetValue<string>("FilArkiv:ClientId"));
        var clientSecret = Guard.Against.NullOrEmpty(_configuration.GetValue<string>("FilArkiv:ClientSecret"));
        var tokenEndpoint = Guard.Against.NullOrEmpty(_configuration.GetValue<string>("FilArkiv:TokenEndpoint"));
        var scope = Guard.Against.NullOrEmpty(_configuration.GetValue<string>("FilArkiv:Scope"));

        await FilArkivCoreClient.ApplyTokenAsync(clientId, clientSecret, tokenEndpoint, scope);
    }
}
