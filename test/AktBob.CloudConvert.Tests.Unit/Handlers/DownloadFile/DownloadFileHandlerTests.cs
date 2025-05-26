using AktBob.CloudConvert.Client;
using AktBob.CloudConvert.Handlers.DownloadFile;
using ErrorOr;
using FluentAssertions;
using NSubstitute;
using System.Text;

namespace AktBob.CloudConvert.Tests.Unit.Handlers.DownloadFile;

public class DownloadFileHandlerTests
{
    private readonly ICloudConvertClient _cloudConvertClient = Substitute.For<ICloudConvertClient>();
    private readonly DownloadFileHandler _sut;
    public DownloadFileHandlerTests()
    {
        _sut = new DownloadFileHandler(_cloudConvertClient);
    }

    [Fact]
    public async Task Handle_ShouldReturnBytes_WhenDownloadIsSuccessful()
    {
        // Arrange
        var expected = Encoding.UTF8.GetBytes("some content");
        _cloudConvertClient.GetFile(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(ErrorOrFactory.From(expected));

        // Act
        var result = await _sut.Handle("http://localhost", CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeSameAs(expected);
    }


    [Fact]
    public async Task Handle_ShouldReturnError_WhenDownloadFails()
    {
        // Arrange
        _cloudConvertClient.GetFile(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(Error.Failure());

        // Act
        var result = await _sut.Handle("http://localhost", CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
    }

}
