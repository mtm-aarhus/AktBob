using ErrorOr;
using Microsoft.Extensions.Logging;

namespace AktBob.Shared.Extensions;
public static class ErrorOrExtensions
{
    public static string ToCommaDelimitedString(this List<Error> errors) => string.Join(", ", errors.Select(e => $"{Enum.GetName(e.Type)} {e.Code}: {e.Description}").ToArray());
    
    public static void LogResultErrors<T>(this ErrorOr<T> result, ILogger logger)
    {
        if (!result.IsError) return;
        
        switch (result.FirstError.Type)
        {
            case ErrorType.Failure:
            case ErrorType.Unexpected:
            case ErrorType.Validation:
            case ErrorType.Conflict:
            case ErrorType.Unauthorized:
            case ErrorType.Forbidden:
                foreach (var error in result.Errors) logger.LogError("{errorType} {code}: {description}", Enum.GetName(error.Type), error.Code, error.Description);
                break;
            
            case ErrorType.NotFound:
            default:
                foreach (var error in result.Errors) logger.LogWarning("{errorType} {code}: {description}", Enum.GetName(error.Type), error.Code, error.Description);
                break;
        }
    }
}
