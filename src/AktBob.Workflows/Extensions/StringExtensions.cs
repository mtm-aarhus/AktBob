using System.Text.RegularExpressions;

namespace AktBob.Workflows.Extensions;
internal static class StringExtensions
{
    public static string ReplacePlaceholders(this string? input, Dictionary<string, string> values)
    {
        if (input == null)
        {
            return string.Empty;
        }

        return Regex.Replace(input, "{{(.*?)}}", match =>
        {
            string key = match.Groups[1].Value;
            return values.ContainsKey(key) ? values[key] : match.Value; // Keep placeholder if key not found
        });
    }

    public static bool IsNovaCase(this string caseNumber)
    {
        string pattern = @"^[A-Za-z]\d{4}-\d{1,10}$";
        Regex regex = new Regex(pattern);
        return regex.IsMatch(caseNumber);
    }
}
