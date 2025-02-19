namespace AktBob.JobHandlers.Handlers.CheckOCRScreeningStatus;
internal class CheckOCRScreeningStatusSettings(IConfiguration configuration)
{
    private readonly IConfiguration _configuration = configuration;

    public bool UpdatePodioItemImmediately => _configuration.GetValue<bool?>("CheckOCRScreeningStatus:UpdatePodioItemSetFilArkivUrlImmediately") ?? false;
}
