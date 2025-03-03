namespace AktBob.Shared.Extensions;
public static class DateTimeExtensions
{
    // Get utc time and convert to Danish time zone
    public static DateTime UtcToDanish(this DateTime ts)
    {
        DateTime createdAtUtc = DateTime.SpecifyKind(ts, DateTimeKind.Utc);
        TimeZoneInfo tzi = TimeZoneInfo.FindSystemTimeZoneById("Romance Standard Time");
        DateTime createdAtDanishTime = TimeZoneInfo.ConvertTimeFromUtc(createdAtUtc, tzi);

        return createdAtDanishTime;
    }
}
