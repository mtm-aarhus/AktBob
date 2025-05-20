using AktBob.CloudConvert.Models.JobResponse;
using AktBob.Tests.Unit.Shared;
using FluentAssertions;
using System.Net;
using System.Text;
using System.Text.Json;

namespace AktBob.CloudConvert.Tests.Unit;

public class CloudConvertClientTests
{
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
        var sut = new CloudConvertClient(httpClient);

        // Act
        var result = await sut.CreateJob(new { }, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(expectedResponse.Data.Id);
    }


    [Theory]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.InternalServerError)]
    [InlineData(HttpStatusCode.Unauthorized)]
    public async Task CreateJob_ShouldThrowException_WhenResponseIsNotSuccessful(HttpStatusCode statusCode)
    {
        // Arrange
        var response = new {};
        var responseMessage = HttpClientHelper.CreateResponseMessageWithStringContent(statusCode, response);
        var httpClient = HttpClientHelper.CreateClientThatReturns(responseMessage);
        var sut = new CloudConvertClient(httpClient);

        // Act
        var act = () => sut.CreateJob(new { }, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>();
    }



    [Fact]
    public async Task CreateJob_ShouldThrowException_WhenResponseIsNull()
    {
        // Arrange
        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = null
        };
        var httpClient = HttpClientHelper.CreateClientThatReturns(responseMessage);
        var sut = new CloudConvertClient(httpClient);

        // Act
        var act = () => sut.CreateJob(new { }, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<JsonException>();
    }


    [Fact]
    public async Task CreateJob_Should_RethrowException_WhenExceptionIsThrown()
    {
        // Arrange
        var httpException = new HttpRequestException();
        var httpClient = HttpClientHelper.CreateClientThatThrows(httpException);
        var sut = new CloudConvertClient(httpClient);

        // Act
        var act = () => sut.CreateJob(new { }, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>();
    }


    [Fact]
    public async Task GetJob_ShouldReturnJobResponse_WhenRequestIsSuccessful()
    {
        // Arrange
        var jobId = Guid.NewGuid();
        var response = new JobResponseRoot { Data = new JobResponseData { Id = jobId } };
        var responseMessage = HttpClientHelper.CreateResponseMessageWithStringContent(HttpStatusCode.OK, response);
        var httpClient = HttpClientHelper.CreateClientThatReturns(responseMessage);
        var sut = new CloudConvertClient(httpClient);

        // Act
        var result = await sut.GetJob(jobId, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().NotBeNull();
        result.Value.Data.Id.Should().Be(jobId);
    }


    [Fact]
    public async Task GetJob_ShouldThrowException_WhenJobIsNotFound()
    {
        // Arrange
        var responseMessage = new HttpResponseMessage(HttpStatusCode.NotFound);
        var httpClient = HttpClientHelper.CreateClientThatReturns(responseMessage);
        var sut = new CloudConvertClient(httpClient);

        // Act
        var act = () => sut.GetJob(Guid.NewGuid(), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>();
    }


    [Fact]
    public async Task GetJob_ShouldRethrowException_WhenExceptionIsThrown()
    {
        // Arrange
        var httpException = new HttpRequestException();
        var httpClient = HttpClientHelper.CreateClientThatThrows(httpException);
        var sut = new CloudConvertClient(httpClient);

        // Act
        var act = () => sut.GetJob(Guid.NewGuid(), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>();
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
        var sut = new CloudConvertClient(httpClient);

        // Act
        var result = await sut.GetFile("https://localhost", CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().NotBeNull();
        result.Value.Should().BeOfType(typeof(byte[]));
        result.Value.Should().BeEquivalentTo(expectedBytes);
    }


    [Fact]
    public async Task GetFile_ShouldThrowException_WhenFileIsNotFound()
    {
        // Arrange
        var responseMessage = new HttpResponseMessage(HttpStatusCode.NotFound);
        var httpClient = HttpClientHelper.CreateClientThatReturns(responseMessage);
        var sut = new CloudConvertClient(httpClient);

        // Act
        var act = () => sut.GetFile("http://localhost", CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>();
    }


    [Fact]
    public async Task GetFile_ShouldRethrowException_WhenExceptionIsThrown()
    {
        // Arrange
        var httpException = new HttpRequestException();
        var httpClient = HttpClientHelper.CreateClientThatThrows(httpException);
        var sut = new CloudConvertClient(httpClient);

        // Act
        var act = () => sut.GetFile("http://localhost", CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>();
    }
}