using System.Globalization;

namespace AktBob.Shared.Extensions;

public static class StringExtensions
{
    public static bool TryParseDeskproDateTime(this string? input, out DateTime? parsedDateTime)
    {
        var dateFormat = "yyyy-MM-dd'T'HH:mm:ssK";
        var alternateDateFormat = "yyyy-MM-dd'T'HH:mm:ss+zzzz";  // Alternate format with no colon in offset

        if (DateTime.TryParseExact(input, dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out var date))
        {
            parsedDateTime = date;
            return true;
        }
        else if (DateTime.TryParseExact(input, alternateDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out date))
        {
            parsedDateTime = date;
            return true;
        }

        parsedDateTime = null;
        return false;
    }
}