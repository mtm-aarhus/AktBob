using AktBob.CloudConvert.Handlers.DownloadFile;
using ErrorOr;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using NSubstitute;

namespace AktBob.CloudConvert.Tests.Unit.Handlers.DownloadFile;
public class DownloadFileHandlerLoggingTests
{
    private readonly DownloadFileHandlerLogging _sut;
    private readonly IDownloadFileHandler _inner = Substitute.For<IDownloadFileHandler>();
    private readonly FakeLogger<DownloadFileHandler> _logger = new FakeLogger<DownloadFileHandler>();

    public DownloadFileHandlerLoggingTests()
    {
        _sut = new DownloadFileHandlerLogging(_inner, _logger);
    }

    [Fact]
    public async Task DownloadFile_ShouldReturnResult_WhenInvoked()
    {
        // Arrange
        var innerResult = ErrorOrFactory.From(new byte[] { });

        _inner
            .Handle(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(innerResult));

        // Act
        var result = await _sut.Handle(string.Empty, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Should().Be(innerResult);
    }

    [Fact]
    public async Task DownloadFile_ShouldLogInformation_WhenInvoked()
    {
        // Arrange
        var innerResult = ErrorOrFactory.From(new byte[] { });

        _inner
            .Handle(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(innerResult));

        // Act
        await _sut.Handle(string.Empty, CancellationToken.None);

        // Assert
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Information);
    }

    [Fact]
    public async Task DownloadFile_ShouldLogWarning_WhenResultIsNotSuccesful()
    {
        // Arrange
        _inner
            .Handle(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Error.Failure().ToErrorOr<byte[]>());

        // Act
        var result = await _sut.Handle(string.Empty, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Warning);
    }
}
