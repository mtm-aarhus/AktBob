using AktBob.CloudConvert.Models.JobResponse;
using AktBob.Tests.Unit.Shared;
using Ardalis.Result;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using NSubstitute;
using System.Formats.Asn1;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AktBob.CloudConvert.Tests.Unit;

public class CloudConvertClientTests
{
    private readonly FakeLogger<CloudConvertClient> _logger;
    public CloudConvertClientTests()
    {
        _logger = new FakeLogger<CloudConvertClient>();
    }

    [Fact]
    public async Task CreateJob_Should_ReturnJobId_WhenResponseIsSuccessful()
    {
        // Arrange
        var response = new JobResponseRoot
        {
            Data = new JobResponseData
            {
                Id = Guid.NewGuid()
            }
        };
        var responseContent = JsonSerializer.Serialize(response);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
        };
        var httpMessageHandler = new MockHttpMessageHandler((request, cancellationToken) => Task.FromResult(httpResponse));
        var httpClient = new HttpClient(httpMessageHandler) { BaseAddress = new Uri("http://localhost")};
        var sut = new CloudConvertClient(httpClient, _logger);

        // Act
        var result = await sut.CreateJob(new { });

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(response.Data.Id);

    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.InternalServerError)]
    [InlineData(HttpStatusCode.Unauthorized)]
    public async Task Should_ReturnError_WhenResponseIsNotSuccessful(HttpStatusCode statusCode)
    {
        // Arrange
        var response = new {};
        var responseContent = JsonSerializer.Serialize(response);
        var httpResponse = new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
        };
        var messageHandler = new MockHttpMessageHandler((request, cancellationToken) => Task.FromResult(httpResponse));
        var httpclient = new HttpClient(messageHandler) { BaseAddress = new Uri("http://localhost") };
        var sut = new CloudConvertClient(httpclient, _logger);

        // Act
        var result = await sut.CreateJob(new { });

        // Assert
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Should_ReturnError_WhenResponseIsNull()
    {
        // Arrange
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = null
        };
        var messageHandler = new MockHttpMessageHandler((request, cancellationToken) => Task.FromResult(httpResponse));
        var httpclient = new HttpClient(messageHandler) { BaseAddress = new Uri("http://localhost") };
        var sut = new CloudConvertClient(httpclient, _logger);

        // Act
        var result = await sut.CreateJob(new { });

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Error);
    }

    [Fact]
    public void Should_ReturnError_WhenExceptionIsThrown()
    {
        
    }
}

/*
 * 
 * Should_ReturnJobId_WhenResponseIsSuccessful
Mocks a successful HTTP response.
Ensures the method logs success and returns the correct Id.
2. Should_ReturnError_WhenHttpResponseIsNotSuccessful
Mocks a failed HTTP response (e.g., 500 Internal Server Error).
Ensures the method logs an error and returns Result.Error().
3. Should_ReturnError_WhenDeserializedDataIsNull
Mocks a successful response but with null data.
Ensures it logs an error and returns Result.Error().
4. Should_ReturnError_WhenExceptionIsThrown
Simulates an exception during execution.
Ensures it logs the exception and returns Result.Error().

*/