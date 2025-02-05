using Ardalis.Result;
using FastEndpoints;

namespace AktBob.Database.Extensions;
internal static class EndpointExtensions
{
    public static async Task SendResponse<TResult, TResponse>(this IEndpoint endpoint, TResult result, Func<TResult, TResponse> mapper, CancellationToken cancellationToken = default) where TResult : IResult
    {
        switch (result.Status)
        {
            case ResultStatus.Ok:
                
                await endpoint.HttpContext.Response.SendAsync(mapper(result), cancellation: cancellationToken);
                break;


            case ResultStatus.CriticalError:
                
                foreach (var error in result.ValidationErrors)
                {
                    endpoint.ValidationFailures.Add(new(error.Identifier, error.ErrorMessage));
                }
                await endpoint.HttpContext.Response.SendErrorsAsync(endpoint.ValidationFailures, 500, cancellation: cancellationToken);
                break;


            case ResultStatus.Error:
            case ResultStatus.Invalid:

                foreach (var error in result.ValidationErrors)
                {
                    endpoint.ValidationFailures.Add(new(error.Identifier, error.ErrorMessage));
                }
                await endpoint.HttpContext.Response.SendErrorsAsync(endpoint.ValidationFailures, 400, cancellation: cancellationToken);
                break;


            case ResultStatus.NotFound:
            default:
                await endpoint.HttpContext.Response.SendNotFoundAsync(cancellation: cancellationToken);
                break;
        }
    }
}