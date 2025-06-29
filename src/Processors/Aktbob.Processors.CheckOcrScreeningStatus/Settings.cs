using Microsoft.Extensions.Configuration;

namespace Aktbob.Processors.CheckOcrScreeningStatus;

internal static class Settings
{
    public static bool ShouldPodioItemBeUpdatedImmediately(IConfiguration configuration) => configuration.GetValue<bool?>("CheckOcrScreeningStatus:UpdatePodioItemSetFilArkivUrlImmediately") ?? false;
}
