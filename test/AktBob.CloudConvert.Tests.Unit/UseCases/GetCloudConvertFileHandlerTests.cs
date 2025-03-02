using AktBob.CloudConvert.Handlers;
using Ardalis.Result;
using FluentAssertions;
using NSubstitute;
using System.Text;

namespace AktBob.CloudConvert.Tests.Unit.UseCases;

public class GetCloudConvertFileHandlerTests
{
    private readonly ICloudConvertClient _cloudConvertClient = Substitute.For<ICloudConvertClient>();
    private readonly GetCloudConvertFileHandler _sut;
    public GetCloudConvertFileHandlerTests()
    {
        _sut = new GetCloudConvertFileHandler(_cloudConvertClient);
    }

    [Fact]
    public async Task Handle_ShouldReturnBytes_WhenDownloadIsSuccessful()
    {
        // Arrange
        var expected = Encoding.UTF8.GetBytes("some content");
        _cloudConvertClient.GetFile(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(Result.Success(expected));

        // Act
        var result = await _sut.Handle("http://localhost", CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeSameAs(expected);
    }


    [Fact]
    public async Task Handle_ShouldReturnError_WhenDownloadFails()
    {
        // Arrange
        _cloudConvertClient.GetFile(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(Result.Error());

        // Act
        var result = await _sut.Handle("http://localhost", CancellationToken.None);
        
        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Error);
    }

}
