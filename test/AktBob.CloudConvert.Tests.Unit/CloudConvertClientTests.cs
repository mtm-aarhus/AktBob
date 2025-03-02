using AktBob.CloudConvert.Models.JobResponse;
using AktBob.Tests.Unit.Shared;
using Ardalis.Result;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using System.Net;
using System.Text;

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
        var expectedResponse = new JobResponseRoot
        {
            Data = new JobResponseData
            {
                Id = Guid.NewGuid()
            }
        };
        var responseMessage = HttpClientHelper.CreateResponseMessageWithStringContent(HttpStatusCode.OK, expectedResponse);
        var httpClient = HttpClientHelper.CreateClientThatReturns(responseMessage);
        var sut = new CloudConvertClient(httpClient, _logger);

        // Act
        var result = await sut.CreateJob(new { }, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(expectedResponse.Data.Id);
    }


    [Theory]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.InternalServerError)]
    [InlineData(HttpStatusCode.Unauthorized)]
    public async Task CreateJob_Should_ReturnError_WhenResponseIsNotSuccessful(HttpStatusCode statusCode)
    {
        // Arrange
        var response = new {};
        var responseMessage = HttpClientHelper.CreateResponseMessageWithStringContent(statusCode, response);
        var httpClient = HttpClientHelper.CreateClientThatReturns(responseMessage);
        var sut = new CloudConvertClient(httpClient, _logger);

        // Act
        var result = await sut.CreateJob(new { }, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Error);
        _logger.Collector.LatestRecord.Exception.Should().BeOfType(typeof(HttpRequestException));
    }



    [Fact]
    public async Task CreateJob_Should_ReturnError_WhenResponseIsNull()
    {
        // Arrange
        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = null
        };
        var httpClient = HttpClientHelper.CreateClientThatReturns(responseMessage);
        var sut = new CloudConvertClient(httpClient, _logger);

        // Act
        var result = await sut.CreateJob(new { }, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Error);
    }


    [Fact]
    public async Task CreateJob_Should_ReturnError_WhenExceptionIsThrown()
    {
        // Arrange
        var httpException = new HttpRequestException();
        var httpClient = HttpClientHelper.CreateClientThatThrows(httpException);
        var sut = new CloudConvertClient(httpClient, _logger);

        // Act
        var result = await sut.CreateJob(new { }, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Error);
        _logger.Collector.LatestRecord.Exception.Should().Be(httpException);
    }


    [Fact]
    public async Task GetJob_ShouldReturnJobResponse_WhenRequestIsSuccessful()
    {
        // Arrange
        var jobId = Guid.NewGuid();
        var response = new JobResponseRoot { Data = new JobResponseData { Id = jobId } };
        var responseMessage = HttpClientHelper.CreateResponseMessageWithStringContent(HttpStatusCode.OK, response);
        var httpClient = HttpClientHelper.CreateClientThatReturns(responseMessage);
        var sut = new CloudConvertClient(httpClient, _logger);

        // Act
        var result = await sut.GetJob(jobId, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Data.Id.Should().Be(jobId);
    }


    [Fact]
    public async Task GetJob_ShouldReturnError_WhenJobIsNotFound()
    {
        // Arrange
        var responseMessage = new HttpResponseMessage(HttpStatusCode.NotFound);
        var httpClient = HttpClientHelper.CreateClientThatReturns(responseMessage);
        var sut = new CloudConvertClient(httpClient, _logger);

        // Act
        var result = await sut.GetJob(Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Error);
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Error);
        _logger.Collector.LatestRecord.Exception.Should().BeOfType(typeof(HttpRequestException));
    }


    [Fact]
    public async Task GetJob_ShouldReturnError_WhenExceptionIsThrown()
    {
        // Arrange
        var httpException = new HttpRequestException();
        var httpClient = HttpClientHelper.CreateClientThatThrows(httpException);
        var sut = new CloudConvertClient(httpClient, _logger);

        // Act
        var result = await sut.GetJob(Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Error);
        _logger.Collector.LatestRecord.Exception.Should().Be(httpException);
    }


    [Fact]
    public async Task GetFile_ShouldReturnFileObject_WhenRequestIsSuccessful()
    {
        // Arrange
        var expectedBytes = Encoding.UTF8.GetBytes("File content");
        var streamContent = new MemoryStream(expectedBytes);
        var responseMessage = new HttpResponseMessage(statusCode: HttpStatusCode.OK)
        {
            Content = new StreamContent(streamContent)
        };
        
        var httpClient = HttpClientHelper.CreateClientThatReturns(responseMessage);
        var sut = new CloudConvertClient(httpClient, _logger);

        // Act
        var result = await sut.GetFile("https://localhost", CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().BeOfType(typeof(byte[]));
        result.Value.Should().BeEquivalentTo(expectedBytes);
    }


    [Fact]
    public async Task GetFile_ShouldReturnError_WhenFileIsNotFound()
    {
        // Arrange
        var responseMessage = new HttpResponseMessage(HttpStatusCode.NotFound);
        var httpClient = HttpClientHelper.CreateClientThatReturns(responseMessage);
        var sut = new CloudConvertClient(httpClient, _logger);

        // Act
        var result = await sut.GetFile("http://localhost", CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Error);
        _logger.Collector.LatestRecord.Exception.Should().BeOfType(typeof(HttpRequestException));
    }


    [Fact]
    public async Task GetFile_ShouldReturnError_WhenExceptionIsThrown()
    {
        // Arrange
        var httpException = new HttpRequestException();
        var httpClient = HttpClientHelper.CreateClientThatThrows(httpException);
        var sut = new CloudConvertClient(httpClient, _logger);

        // Act
        var result = await sut.GetFile("http://localhost", CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Error);
        _logger.Collector.LatestRecord.Exception.Should().Be(httpException);
    }
}