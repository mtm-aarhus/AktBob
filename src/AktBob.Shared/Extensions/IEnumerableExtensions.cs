namespace AktBob.Shared.Extensions;

public static class EnumerableExtensions
{
    public static string AsString(this IEnumerable<string> values) => string.Join(',', values);
}
