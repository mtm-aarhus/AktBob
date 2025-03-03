namespace AktBob.JobHandlers.Handlers.CheckOCRScreeningStatus;

internal class Settings(IConfiguration configuration)
{
    private readonly IConfiguration _configuration = configuration;

    public bool UpdatePodioItemImmediately => _configuration.GetValue<bool?>("CheckOCRScreeningStatus:UpdatePodioItemSetFilArkivUrlImmediately") ?? false;
}
