using AktBob.CloudConvert.Handlers.DownloadFile;
using ErrorOr;
using FluentAssertions;
using Microsoft.Extensions.Logging.Testing;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace AktBob.CloudConvert.Tests.Unit.Handlers.DownloadFile;
public class DownloadFileHandlerExceptionTests
{
    private readonly DownloadFileHandlerException _sut;
    private readonly IDownloadFileHandler _inner = Substitute.For<IDownloadFileHandler>();
    private readonly FakeLogger<DownloadFileHandler> _logger = new FakeLogger<DownloadFileHandler>();

    public DownloadFileHandlerExceptionTests()
    {
        _sut = new DownloadFileHandlerException(_inner, _logger);
    }

    [Fact]
    public async Task DownloadFile_ShouldReturnResult_WhenInnerModuleSucceeds()
    {
        // Arrange
        var innerResult = ErrorOrFactory.From(new byte[] { });
        _inner
            .Handle(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(innerResult));

        // Act
        var result = await _sut.Handle(string.Empty, CancellationToken.None);

        // Assert
        result.Should().Be(innerResult);
    }


    [Fact]
    public async Task DownloadFile_ShouldReturnError_WhenInnerModuleFails()
    {
        // Arrange
        _inner
            .Handle(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .ThrowsAsync<Exception>();

        // Act
        var result = await _sut.Handle(string.Empty, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        _logger.Collector.LatestRecord.Level.Should().Be(Microsoft.Extensions.Logging.LogLevel.Critical);
    }

}
