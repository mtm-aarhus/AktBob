namespace AktBob.Workflows.Processes.CheckOCRScreeningStatus;

internal static class Settings
{
    public static bool ShouldUpdatePodioItemImmediately(IConfiguration configuration) => configuration.GetValue<bool?>("CheckOCRScreeningStatus:UpdatePodioItemSetFilArkivUrlImmediately") ?? false;
}
