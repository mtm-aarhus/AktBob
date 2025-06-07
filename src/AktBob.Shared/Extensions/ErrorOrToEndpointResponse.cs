using ErrorOr;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AktBob.Shared.Extensions;

public static class ErrorOrToEndpointResponse
{
    public static IResult ToMinimalApiResponse<TResult>(
        this ErrorOr<TResult> result)
    {
        return result.Match(
            value => Results.NoContent(),
            MapErrors
        );
    }

    public static IResult ToMinimalApiResponse<TResult, TResponse>(
        this ErrorOr<TResult> result,
        Func<TResult, TResponse> mapper,
        Func<TResult, string>? routeName = null)
    {
        return result.Match(
            value =>
            {
                var response = mapper(value);

                if (routeName is null)
                {
                    return Results.Ok(response);
                }
                
                return Results.CreatedAtRoute(routeName(value), response);
            },

            MapErrors
        );
    }

    private static IResult MapErrors(List<Error> errors)
    {
        if (errors.Count == 0)
        {
            return Results.Problem(title: "Unknown error", statusCode: 500);
        }

        var error = errors.First();

        if (error.Type == ErrorType.Validation)
        {
            var errorDictionary = errors
                    .GroupBy(e => e.Code)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.Description).ToArray());

            return Results.ValidationProblem(errorDictionary);
        }

        var problem = new ProblemDetails
        {
            Title = error.Type.ToString(),
            Detail = string.Join("; ", errors.Select(e => $"{e.Code}: {e.Description}")),
            Status = error.Type switch
            {
                ErrorType.Conflict => StatusCodes.Status409Conflict,
                ErrorType.NotFound => StatusCodes.Status404NotFound,
                ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
                ErrorType.Forbidden => StatusCodes.Status403Forbidden,
                _ => StatusCodes.Status500InternalServerError
            }
        };

        return Results.Problem(problem);
    }
}