using ErrorOr;

namespace AktBob.Shared.Extensions;
public static class ErrorOrExtensions
{
    public static string ToCommaDelimitedString(this List<Error> errors) => string.Join(", ", errors.Select(e => $"{e.Code}: {e.Description}").ToArray());
}
