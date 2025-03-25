using System.Net;
using System.Text.Json;
using System.Text;

namespace AktBob.Tests.Unit.Shared;
public static class HttpClientHelper
{
    public static HttpClient CreateClientThatReturns(HttpResponseMessage responseMessage)
    {
        var messageHandler = new MockHttpMessageHandler((request, cancellationToken) => Task.FromResult(responseMessage));
        return CreateClient(messageHandler);
    }


    public static HttpClient CreateClientThatThrows(Exception exception)
    {
        var messageHandler = new MockHttpMessageHandler((request, cancellationToken) => throw exception);
        return CreateClient(messageHandler);
    }


    private static HttpClient CreateClient(MockHttpMessageHandler messageHandler) => new HttpClient(messageHandler) { BaseAddress = new Uri("http://localhost") };


    public static HttpResponseMessage CreateResponseMessageWithStringContent(HttpStatusCode statusCode, object? content)
    {
        var responseMessage = new HttpResponseMessage(statusCode);
        
        if (content != null)
        {
            var json = JsonSerializer.Serialize(content);
            responseMessage.Content = new StringContent(json, Encoding.UTF8, "application/json");
        }

        return responseMessage;
    }
}
